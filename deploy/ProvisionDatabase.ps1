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
 param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "app_service_name"
}
function Get-SqlServerNameArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "sql_server_name"
}
function Get-SqlServerUserNameArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "sql_server_admin_login"
}
function Get-SqlServerPasswordArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "sql_server_admin_login_password"
}
function Get-SqlDbNameArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "sql_database_name"
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
	$parameters = Get-Content ($scriptPath + "\" + $config.ARMParametersFileBackend) -Raw -ErrorAction Stop | ConvertFrom-Json
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
		WriteE -message "Solution installation failed."
	}
	else {
		
		WriteS -message "Solution back-end install script finished. Next steps:"
		WriteS -message "1. Check for any errors above."
		WriteS -message "2. Build and publish docker image."
		WriteS -message "3. Deploy app service using published docker image."
	}
}

function InstallAzComponents {
	param (
		[Parameter(Mandatory = $true)] $config
	)
	
	$firewallConfigured = AddClientPublicIpToSqlFirewall $config
	TriggerAppServiceWebJob $config (Get-AppServiceNameArmTemplateValue $config)
	
	# Wait for the webjob to finish initialising
	WriteI -message "Waiting for the webjob to finish initialising SQL database..."
	Start-Sleep -Seconds 30
	
	# Make sure we have at least one site to filter, otherwise webjob won't run
	if ($firewallConfigured -eq $true) {
		# Add the URL filters to the database
		AddUrlFiltersToDB $config

		# Trigger the webjob again now we have the filters so it'll start processing
		TriggerAppServiceWebJob $config (Get-AppServiceNameArmTemplateValue $config)
	}
	else {
		WriteE -message "Error: Unable to add URL filters to the database. Please add manually to table 'import_url_filter'"
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
	if ($null -eq $config.ARMParametersFileBackend) {
		WriteE "Error: ARMParametersFileBackend is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}

	if ($null -eq $config.RecordAuditEventsOnSites -or $config.RecordAuditEventsOnSites.Count -eq 0) {
		WriteE "Error: RecordAuditEventsOnSites is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}

	return $true
}

# Load common script functions
$scriptContent = Get-Content -Path .\SharedFunctions.ps1 -Raw
Invoke-Expression $scriptContent

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