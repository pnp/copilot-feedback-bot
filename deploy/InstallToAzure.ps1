#
# InstallToAzure.ps1
#
# Installs the solution to Azure using the provided configuration file.

Param 
(
	$ConfigFileName = "InstallConfig.json", 	
	[switch] $UninstallOnly
)

function Get-ScriptDirectory {
	$Invocation = (Get-Variable MyInvocation -Scope 1).Value
	Split-Path $Invocation.MyCommand.Path
}


function Get-AppServiceNameArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "app_service_name"
}
function Get-FunctionAppServiceNameArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "function_app_service_name"
}
function Get-ArmTemplateValue {
	param (
		$config,
		$parameterName
	)

	if ($null -eq $config) {
		WriteE -message "Error: Configuration object is null." 
		return
	}
	if ($null -eq $config.ARMParametersFile) {
		WriteE "Error: ARMParametersFile value is null."
		return
	}

	$parametersContent = Get-Content ($scriptPath + "\" + $config.ARMParametersFile) -Raw -ErrorAction Stop | ConvertFrom-Json

	return $parametersContent.parameters.$parameterName.value
}

# write information
function WriteI {
	param(
		[parameter(mandatory = $true)]
		[string]$message
	)
	Write-Host $message -foregroundcolor white
}

# write error
function WriteE {
	param(
		[parameter(mandatory = $true)]
		[string]$message
	)
	Write-Host $message -foregroundcolor red -BackgroundColor black
}

# write warning
function WriteW {
	param(
		[parameter(mandatory = $true)]
		[string]$message
	)
	Write-Host $message -foregroundcolor yellow -BackgroundColor black
}

# write success
function WriteS {
	param(
		[parameter(mandatory = $true)]
		[string]$message
	)
	Write-Host $message -foregroundcolor green -BackgroundColor black
}



# Install custom action in all sites listed in the config
function ValidateAndInstall ($configFileName) {

	# Load config and sanity check
	try {
		$config = Get-Content ($scriptPath + "\" + $configFileName) -Raw -ErrorAction Stop | ConvertFrom-Json
		Write-Host ("Read configuration for '" + ($configFileName) + "'...")
	}
	catch {
		WriteE -message "FATAL ERROR: Cannot open config-file '$configFileName'"
		return
	}

	if (-not (ValidateConfig $config)) {
		return
	}

	# Validate ARM params
	$parameters = Get-Content ($scriptPath + "\" + $config.ARMParametersFile) -Raw -ErrorAction Stop | ConvertFrom-Json
	if ($null -eq $parameters) {
		WriteE -message "Error: ARM parameters file not found or valid JSon." 
		return
	}

	# Get the list of all subscriptions
	$subscriptions = Get-AzSubscription

	# Check if the subscription exists
	$subscriptionExists = $subscriptions | Where-Object { $_.Id -eq $config.SubcriptionId }

	$subId = $config.SubcriptionId
	if ($null -eq $subscriptionExists) {
		WriteE -message "Error: Subscription with ID $subId not found."
		return
	}
	else {
		WriteS -message "Subscription with ID $subId exists."
	}

	# Select the subscription
	WriteI -message "Selecting subscription with ID $subId..."
	Select-AzSubscription -SubscriptionId $subId
	$resourceGroup = Get-AzResourceGroup -Name $config.ResourceGroupName -ErrorAction SilentlyContinue

	if ($null -eq $resourceGroup) {
		# Create the resource group if it doesn't exist
		New-AzResourceGroup -Name $config.ResourceGroupName -Location $config.ResourceGroupLocation
		WriteS -message "Resource group '$($config.ResourceGroupName)' created in location '$($config.ResourceGroupLocation)'." 
	}
	else {
		WriteS -message "Resource group '$($config.ResourceGroupName)' already exists."
	}

	InstallAzComponents($config)
}

function RemoveDeployment {
	param (
		[Parameter(Mandatory = $true)] $config,
		$webAppName
	)
	$uri = "/subscriptions/$($config.SubcriptionId)/resourceGroups/$($config.ResourceGroupName)/providers/Microsoft.Web/sites/$webAppName/config/web?api-version=2023-12-01"
	$responseRaw = Invoke-AzRestMethod -Method PATCH -Path $uri `
		-Payload (@{ properties = @{ scmType = "None" } } | ConvertTo-Json) -Verbose

	if ($responseRaw.StatusCode -eq 200) {
		WriteS "Deployment removed successfully."
	}
	else {
		if ($responseRaw.StatusCode -eq 404) {
			WriteW -message "Deployment not found." 
		}
		else {
			WriteE -message "Error: Deployment removal failed. Response: $($responseRaw.Content)" 
		}
	}
}

function InstallAzComponents {
	param (
		[Parameter(Mandatory = $true)] $config
	)

	WriteI "Removing previous App Service deployments..." 
	$webAppName = Get-AppServiceNameArmTemplateValue $config
	$funcWebAppName = Get-FunctionAppServiceNameArmTemplateValue $config

	RemoveDeployment $config $webAppName	
	RemoveDeployment $config $funcWebAppName
	
	# Deploy the ARM template
	$deploySuccess = DeployARMTemplate $config
	if (-not $deploySuccess) {
		# Wait for the code deployment to sync
		WriteI -message "Waiting for code deployment to sync..." 

		$appServicesNames = [System.Collections.ArrayList]@(
			$webAppName, 
			$funcWebAppName
		)

		# wait couple of minutes & check deployment status...
		$appserviceCodeSyncSuccess = WaitForCodeDeploymentSync $config $appServicesNames.Clone()
                
		if ($appserviceCodeSyncSuccess) {
			WriteE -message "ARM deploy failed but app service deploy jobs succeeded. Re-running deployment in 20 mins as ARM template can fail with long deploy jobs..."
			Start-Sleep -Seconds 1200
			DeployARMTemplate $config
		}
		else {
			Throw "ERROR: Unkown ARM template deployment error."
		}
	}

	WriteS "Solution installed successfully."
}


function DeployARMTemplate {
	param (
		[Parameter(Mandatory = $true)] $config
	)

	# Deploy the ARM template
	WriteI -message "Deploying ARM template..."
	$templateLocation = "$scriptPath\ARM\template.json"
	$paramsLocation = $scriptPath + "\" + $config.ARMParametersFile

	$armDeploy = New-AzResourceGroupDeployment -ResourceGroupName $config.ResourceGroupName -TemplateFile $templateLocation -TemplateParameterFile $paramsLocation -Name "FeedbackBotDeployment" -Verbose

	if ($null -eq $armDeploy) {
		Throw "Error: ARM template deployment fataly failed to resource group '$($config.ResourceGroupName)'. Check previous errors." 
	}
	if ($armDeploy.ProvisioningState -eq "Succeeded") {
		WriteS "ARM template deployment succeeded to resource group '$($config.ResourceGroupName)'." 
		return $true
	}
	else {
		WriteE -message "ARM template deployment failed. Check messages above to make sure no naming conflict, but for now we'll assume it's because the app services aren't ready" 
		return $false
	}
}

function WaitForCodeDeploymentSync {
	Param(
		[Parameter(Mandatory = $true)] $config,
		[Parameter(Mandatory = $true)] $appServicesNames
	)

	$appserviceCodeSyncSuccess = $true
	while ($appServicesNames.Count -gt 0) {
		WriteI -message "Checking source control deployment progress..."
		For ($i = 0; $i -le $appServicesNames.Count; $i++) {
			$appService = $appServicesNames[$i]
			if ($null -ne $appService) {

				$uri = "https://management.azure.com/subscriptions/$($config.SubcriptionId)/resourcegroups/$($config.ResourceGroupName)/providers/Microsoft.Web/sites/$appService/deployments?api-version=2019-08-01"
				
				$deploymentResponseRaw = Invoke-AzRestMethod -Method GET -Path $uri
				$deploymentResponse = $deploymentResponseRaw.Content | ConvertFrom-Json
				$deploymentsList = $deploymentResponse.value
				if ($deploymentsList.length -eq 0 -or $deploymentsList[0].properties.complete) {
					$appserviceCodeSyncSuccess = $appserviceCodeSyncSuccess -and ($deploymentsList.length -eq 0 -or $deploymentsList[0].properties.status -ne 3) # 3 means sync fail
					$appServicesNames.remove($appService)
					WriteS -message "$appService deployment has finished."
					$i--;
				}
			}
		}

		if ($appServicesNames.Count -gt 0) {
			WriteI -message "Source control deployment is still in progress. Next check in 2 minutes."
			Start-Sleep -Seconds 120
		}
	}


	if ($appserviceCodeSyncSuccess) {
		WriteS -message "Source control deployment is done."
	}
	else {
		WriteE -message "Source control deployment failed."
	}
	return $appserviceCodeSyncSuccess
}

function ValidateConfig {
	param (
		$config
	)

	if ($config.SubcriptionId -eq $null) {
		Write-Host "Error: SubcriptionId is missing in the configuration file." -ForegroundColor Red
		return $false
	}
	if ($config.ResourceGroupName -eq $null) {
		Write-Host "Error: ResourceGroupName is missing in the configuration file." -ForegroundColor Red
		return $false
	}
	if ($config.ResourceGroupLocation -eq $null) {
		Write-Host "Error: ResourceGroupLocation is missing in the configuration file." -ForegroundColor Red
		return $false
	}
	if ($config.ARMParametersFile -eq $null) {
		Write-Host "Error: ARMParametersFile is missing in the configuration file." -ForegroundColor Red
		return $false
	}

	return $true
}

$scriptPath = Get-ScriptDirectory
# Install the Az module if it's not already installed
$moduleInstalled = $false
if ($PSVersionTable.PSVersion.Major -lt 7) {
	if (Get-InstalledModule -Name Az -ErrorAction SilentlyContinue) {
		$moduleInstalled = $true
	}
}
else {
	if (Get-Module -ListAvailable -Name Az) {
		$moduleInstalled = $true
	} 
}

if (-not $moduleInstalled) {
	WriteE -message "Az PowerShell module not found. Please install it using 'Install-Module -Name Az -AllowClobber -Scope CurrentUser'." 
	WriteI -message "Documentation: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell"
}
else {
	WriteI -message "Az PowerShell module found. Checking for Azure login context..."
	
	# Check if there is an Azure context
	$context = Get-AzContext

	if ($null -eq $context) {
		WriteE -message "No Azure login context found. Please log in using Connect-AzAccount." 
	}
	else {
		$accountId = $context.Account.Id
		WriteS -message "Azure login context found for account '$accountId'. Installing solution..."
		
		ValidateAndInstall($ConfigFileName)
	}
}