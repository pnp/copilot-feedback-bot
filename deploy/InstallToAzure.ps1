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


function Get-SqlServerArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "sql_server_name"
}
function Get-SqlServerPasswordTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "administratorLoginPassword"
}
function Get-SqlDbArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "sql_database_name"
}

function Get-RedisArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "redis_account_name"
}
function Get-StorageArmTemplateValue {
 param ( $config )
	return Get-ArmTemplateValue $config "storage_account_name"
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
		Write-Host "Error: Configuration object is null." -ForegroundColor Red
		return
	}
	if ($null -eq $config.ARMParametersFile) {
		Write-Host "Error: ARMParametersFile value is null." -ForegroundColor Red
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
		Write-Host "FATAL ERROR: Cannot open config-file '$configFileName'" -ForegroundColor red -BackgroundColor Black
		return
	}

	if (-not (ValidateConfig $config)) {
		return
	}

	# Validate ARM params
	$parameters = Get-Content ($scriptPath + "\" + $config.ARMParametersFile) -Raw -ErrorAction Stop | ConvertFrom-Json
	if ($null -eq $parameters) {
		Write-Host "Error: ARM parameters file not found or valid JSon." -ForegroundColor Red
		return
	}

	# Get the list of all subscriptions
	$subscriptions = Get-AzSubscription

	# Check if the subscription exists
	$subscriptionExists = $subscriptions | Where-Object { $_.Id -eq $config.SubcriptionId }

	$subId = $config.SubcriptionId
	if ($null -eq $subscriptionExists) {
		Write-Host "Error: Subscription with ID $subId not found." -ForegroundColor Red
		return
	}
	else {
		Write-Host "Subscription with ID $subId exists."
	}

	# Select the subscription
	Write-Host "Selecting subscription with ID $subId..."
	Select-AzSubscription -SubscriptionId $subId
	$resourceGroup = Get-AzResourceGroup -Name $config.ResourceGroupName -ErrorAction SilentlyContinue

	if ($null -eq $resourceGroup) {
		# Create the resource group if it doesn't exist
		New-AzResourceGroup -Name $config.ResourceGroupName -Location $config.ResourceGroupLocation
		Write-Host "Resource group '$($config.ResourceGroupName)' created in location '$($config.ResourceGroupLocation)'." -ForegroundColor Yellow
	}
	else {
		Write-Host "Resource group '$($config.ResourceGroupName)' already exists." -ForegroundColor Green
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
		Write-Host "Deployment removed successfully." -ForegroundColor Green
	}
	else {
		if ($responseRaw.StatusCode -eq 404) {
			Write-Host "Deployment not found." -ForegroundColor Yellow
		}
		else {
			Write-Host "Error: Deployment removal failed. Response: $($responseRaw.Content)" -ForegroundColor Red
		}
	}
}

function InstallAzComponents {
	param (
		[Parameter(Mandatory = $true)] $config
	)

	Write-Host "Removing previous App Service deployments..." -ForegroundColor Yellow
	$webAppName = Get-AppServiceNameArmTemplateValue $config
	$funcWebAppName = Get-FunctionAppServiceNameArmTemplateValue $config

	RemoveDeployment $config $webAppName	
	RemoveDeployment $config $funcWebAppName
	

	# Deploy the ARM template
	
	$deploySuccess = DeployARMTemplate $config
	if (-not $deploySuccess) {
		# Wait for the code deployment to sync
		Write-Host "Waiting for code deployment to sync..." -ForegroundColor Yellow

		$appServicesNames = [System.Collections.ArrayList]@(
			$webAppName, 
			$funcWebAppName
		)

		# wait couple of minutes & check deployment status...
		$appserviceCodeSyncSuccess = WaitForCodeDeploymentSync $config $appServicesNames.Clone()
                
		if ($appserviceCodeSyncSuccess) {
			WriteI -message "ARM deploy failed but app service deploy jobs succeeded. Re-running deployment in 20 mins as ARM template can fail with long deploy jobs..."
			Start-Sleep -Seconds 1200
			DeployARMTemplate $config
		}
		else {
			Throw "ERROR: Unkown ARM template deployment error."
		}
	}

	# Configure app services environment variables
	Write-Host "Reading app services environment variables..." -ForegroundColor Yellow
	$webApp = Get-AzWebApp -ResourceGroupName $config.ResourceGroupName -Name $webAppName
	$funcWebApp = Get-AzWebApp -ResourceGroupName $config.ResourceGroupName -Name $funcWebAppName

	$redisName = Get-RedisArmTemplateValue $config
	$redis = Get-AzRedisCache -ResourceGroupName $config.ResourceGroupName -Name $redisName
	$redisKeys = Get-AzRedisCacheKey -ResourceGroupName $config.ResourceGroupName -Name $redisName

	$storageName = Get-StorageArmTemplateValue $config
	$storage = Get-AzStorageAccount -ResourceGroupName $config.ResourceGroupName -Name $storageName
	$storageKeys = Get-AzStorageAccountKey -ResourceGroupName $config.ResourceGroupName -Name $storageName


	$sqlServer = Get-AzSqlServer -ResourceGroupName $config.ResourceGroupName -ServerName (Get-SqlServerArmTemplateValue $config)
	$sqlDb = Get-AzSqlDatabase -ResourceGroupName $config.ResourceGroupName -ServerName $sqlServer.ServerName -DatabaseName (Get-SqlDbArmTemplateValue $config)
	
	Write-Host "Configuring app services environment variables..." -ForegroundColor Yellow
	$sqlConnectionString = "Server=tcp:$($sqlServer.ServerName).database.windows.net,1433;Initial Catalog=$($sqlDb.DatabaseName);Persist Security Info=False;User ID=$($sqlServer.SqlAdministratorLogin);Password=$(Get-SqlServerPasswordTemplateValue $config);MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
	$redisConnectionString = "$($redis.HostName),abortConnect=false,ssl=true,password=$($redisKeys.PrimaryKey)"
	$storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$($storage.StorageAccountName);AccountKey=$($storageKeys[0].Value);EndpointSuffix=core.windows.net"

	Write-host "SQL Connection String: $sqlConnectionString"
	Write-host "Redis Connection String: $redisConnectionString"
	Write-host "Storage Connection String: $storageConnectionString"
	
	UpdateAzWebAppSettings $webApp $config $sqlConnectionString $redisConnectionString $storageConnectionString
	UpdateAzWebAppSettings $funcWebApp $config $sqlConnectionString $redisConnectionString $storageConnectionString

	Write-Host "Restarting app services..." -ForegroundColor Blue
	Restart-AzWebApp -ResourceGroupName $config.ResourceGroupName -Name $webAppName
	Restart-AzWebApp -ResourceGroupName $config.ResourceGroupName -Name $funcWebAppName


	Write-Host "Solution installed successfully." -ForegroundColor Green
}


function DeployARMTemplate {
	param (
		[Parameter(Mandatory = $true)] $config
	)

	# Deploy the ARM template
	Write-Host "Deploying ARM template..." -ForegroundColor Yellow
	$templateLocation = "$scriptPath\ARM\template.json"
	$paramsLocation = $scriptPath + "\" + $config.ARMParametersFile

	$armDeploy = $null
	try
	{
		$armDeploy = New-AzResourceGroupDeployment -ResourceGroupName $config.ResourceGroupName -TemplateFile $templateLocation -TemplateParameterFile $paramsLocation -Name "FeedbackBotDeployment" -Verbose
	}
	catch {
		Write-Host $_
	  }

	if ($null -eq $armDeploy) {
		Throw "Error: ARM template deployment fataly failed. Check previous errors." 
	}


	if ($armDeploy.ProvisioningState -eq "Succeeded") {
		Write-Host "ARM template deployment succeeded." -ForegroundColor Green
		return $true
	}
	else {
		Write-Host "ARM template deployment failed. Check messages above to make sure no naming conflict, but for now we'll assume it's because the app services aren't ready" -ForegroundColor Red
		return $false
	}
}

function UpdateAzWebAppSettings {
	param (
		$webApp,
		$config,
		$sqlConnectionString, $redisConnectionString, $storageConnectionString
	)

	$webAppSettings = @{}

	$webAppSettings["AppCatalogTeamAppId"] = $config.AppCatalogTeamAppId
	$webAppSettings["AuthConfig:ClientId"] = $config.ClientId
	$webAppSettings["AuthConfig:ClientSecret"] = $config.ClientSecret
	$webAppSettings["AuthConfig:TenantId"] = $config.TenantId

	$webAppSettings["MicrosoftAppId"] = $config.ClientId
	$webAppSettings["MicrosoftAppPassword"] = $config.ClientSecret

	$connectionStrings = @{
		"SQL"     = @{
			"Type"  = "SQLAzure"
			"Value" = $sqlConnectionString
		}
		"Redis"   = @{
			"Type"  = "Custom"
			"Value" = $redisConnectionString
		}
		"Storage" = @{
			"Type"  = "Custom"
			"Value" = $storageConnectionString
		}
	}
	$app = Set-AzWebApp -ResourceGroupName $webApp.ResourceGroup -Name $webApp.Name -AppSettings $webAppSettings -ConnectionStrings $connectionStrings
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
					Write-Host "$appService deployment has finished." -ForegroundColor Green	
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
		WriteI -message "Source control deployment is done."
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

	if ($config.ClientId -eq $null) {
		Write-Host "Error: ClientId is missing in the configuration file." -ForegroundColor Red
		return $false
	}
	if ($config.ClientSecret -eq $null) {
		Write-Host "Error: ClientSecret is missing in the configuration file." -ForegroundColor Red
		return $false
	}
	if ($config.TenantId -eq $null) {
		Write-Host "Error: TenantId is missing in the configuration file." -ForegroundColor Red
		return $false
	}
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
	Write-Host "Az PowerShell module not found. Please install it using 'Install-Module -Name Az -AllowClobber -Scope CurrentUser'." -ForegroundColor Red
	WriteI -message "Documentation: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell"
}
else {
	write-host "Az PowerShell module found. Checking for Azure login context..."
	
	# Check if there is an Azure context
	$context = Get-AzContext

	if ($null -eq $context) {
		Write-Host "No Azure login context found. Please log in using Connect-AzAccount." -ForegroundColor Red
	}
	else {
		$accountId = $context.Account.Id
		Write-Host "Azure login context found for account '$accountId'. Installing solution..."
		
		ValidateAndInstall($ConfigFileName)
	}
}