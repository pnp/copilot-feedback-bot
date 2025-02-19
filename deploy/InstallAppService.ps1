#
# InstallAppService.ps1
#
# Installs the solution compute components to Azure using the provided configuration file.

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
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "app_service_name"
}
function Get-ClientIdArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "service_account_client_id"
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
	$parameters = Get-Content ($scriptPath + "\" + $config.ARMParametersFileAppServices) -Raw -ErrorAction Stop | ConvertFrom-Json
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

	$solutionDeploySuccess = InstallAzComponents($config)
	if (-not $solutionDeploySuccess) {
		WriteE -message "App services installation failed."
	}
	else {
		$appContainer = Get-AzContainerApp -ResourceGroupName $config.ResourceGroupName -Name "web"
		if ($null -eq $appContainer) {
			WriteE -message "Error: App service container not found."
			return
		}
		$appContainerConfig = $appContainer | select -ExpandProperty Configuration | ConvertFrom-Json
		WriteS -message "App services install script finished. Next steps:"
		WriteS -message "1. Check for any errors above."
		WriteS -message "2. Check for any errors in the app service deployment center logs. IMPORTANT: The app service container URL is not yet accessible. It will be accessible after the deployment center finishes deploying the app service container."
		WriteS -message "3. Configure the Entra ID app to include the above FQDN reply URL, and optionally the app URI if you want SSO for Teams."
		WriteS -message "4. Run ProvisionDatabase.ps1 to ensure there is minimum data for filtering. Without records in table 'import_url_filter', the importer will abort."
		Write-Host ""
		WriteI -message "App service container FQDN is: $($appContainerConfig.ingress.fqdn)"
		WriteI -message "App service container URL is: https://$($appContainerConfig.ingress.fqdn)"

		$clientId = Get-ClientIdArmTemplateValue($config)
		WriteW -message "Make sure app ID $($clientId) has the right reply URLs etc for the app service container URL."

	}
}

function InstallAzComponents {
	param (
		[Parameter(Mandatory = $true)] $config
	)
	
	# Deploy the ARM template
	$deploySuccess = DeployARMTemplate $config
	if (-not $deploySuccess) {  
		WriteE -message "Deployment failed. Please check the logs above for more information and try again."
		return $false
	}
	else {
		WriteS -message "ARM template deployment succeeded."
		return $true
	}
}


function DeployARMTemplate {
	param (
		[Parameter(Mandatory = $true)] $config
	)

	# Deploy the ARM template
	WriteI -message "Deploying App Service ARM template..."
	$templateLocation = "$scriptPath\ARM\template-appservices.json"
	$paramsLocation = $scriptPath + "\" + $config.ARMParametersFileAppServices

	$armDeploy = New-AzResourceGroupDeployment -ResourceGroupName $config.ResourceGroupName -TemplateFile $templateLocation -TemplateParameterFile $paramsLocation -Name "FeedbackBotAppServicesDeployment" -Verbose

	if ($null -eq $armDeploy) {
		Throw "Error: ARM template deployment fataly failed to resource group '$($config.ResourceGroupName)'. Check previous errors." 
	}
	if ($armDeploy.ProvisioningState -eq "Succeeded") {
		WriteS "ARM template deployment succeeded to resource group '$($config.ResourceGroupName)'." 
		return $true
	}
	else {
		WriteE -message "ARM template deployment failed. Check messages above to troubleshoot why." 
		return $false
	}
}

function ValidateConfig {
	param (
		$config
	)

	if ($null -eq $config.SubcriptionId) {
		WriteE "Error: SubcriptionId is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}
	if ($null -eq $config.ResourceGroupName) {
		WriteE "Error: ResourceGroupName is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}
	if ($null -eq $config.ResourceGroupLocation) {
		WriteE "Error: ResourceGroupLocation is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}
	if ($null -eq $config.ARMParametersFileAppServices) {
		WriteE "Error: ARMParametersFileAppServices is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}

	if ($null -eq $config.RecordAuditEventsOnSites -or $config.RecordAuditEventsOnSites.Count -eq 0) {
		WriteE "Error: RecordAuditEventsOnSites is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}

	return $true
}

# Set global variables
$scriptPath = Get-ScriptDirectory


# Load common script functions
$scriptContent = Get-Content -Path ($scriptPath + "\SharedFunctions.ps1") -Raw
Invoke-Expression $scriptContent

$azAppInstalled = LoadModuleGetAzContext -moduleName "Az.App" -loadContext $false
if (-not $azAppInstalled) {
	WriteI -message "Documentation: https://learn.microsoft.com/en-us/powershell/module/az.app"
	return
}
Import-Module Az.App

$canInstall = LoadAzModuleGetAzContext
if ($canInstall) {
	ValidateAndInstall($ConfigFileName)
}