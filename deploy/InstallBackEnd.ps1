#
# InstallBackEnd.ps1
#
# Installs the solution back-end components to Azure using the provided configuration file.

Param 
(
	$ConfigFileName = "InstallConfig.json", 	
	[switch] $UninstallOnly
)

function Get-ScriptDirectory {
	$Invocation = (Get-Variable MyInvocation -Scope 1).Value
	Split-Path $Invocation.MyCommand.Path
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
		WriteS -message "2. Verify App registration URI + reply URLs."
		WriteS -message "3. Configure arguments in 'docker-compose.override.yml' for your environment."
		WriteS -message "4. Build docker images with 'docker-compose build' from the 'src' folder"
		WriteS -message "5. Publish docker images (TagAndPushImages.ps1) + follow further instructions to deploy compute components."

		$clientId = Get-ClientIdArmTemplateValue $config
		$frontDoorDns = Get-FrontDoorNameArmTemplateValue $config
		$rootHttps = "https://$($frontDoorDns).azurefd.net"
		WriteW -message "Your public-facing URL will be https://$($frontDoorDns).azurefd.net/"
		WriteW -message "Recommended app URI for client ID '$($clientId)' for Teams App SSO to work: api://$($frontDoorDns).azurefd.net/$($clientId)"

		Write-Host ""
		WriteS -message "Edit 'docker-compose.override.yml' with these arguments for building the JavaScript applications correctly:"
		WriteI -message "VITE_MSAL_CLIENT_ID=$clientId"
		writeI -message "VITE_MSAL_AUTHORITY=https://login.microsoftonline.com/organizations"
		WriteI -message "VITE_API_ENDPOINT=$($rootHttps)"
		WriteI -message "VITE_MSAL_SCOPES=api://$($frontDoorDns).azurefd.net/$clientId/access"
		WriteI -message "VITE_TEAMSFX_START_LOGIN_PAGE_URL=$($rootHttps)/auth-start.html"

		
		Write-Host ""
		WriteS -message "Edit '$($config.ARMParametersFileAppServices)' and set these ARM template parameters in the JSON:"
		WriteI -message "`"api_audience`": 	{ `"value`": `"$(Get-AcrNameArmTemplateValue $config).azurecr.io/copilotbot-web:latest`"		},"
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
	WriteI -message "Deploying Backend ARM template..."
	$templateLocation = "$scriptPath\ARM\template-backend.json"
	$paramsLocation = $scriptPath + "\" + $config.ARMParametersFileBackend

	$armDeploy = New-AzResourceGroupDeployment -ResourceGroupName $config.ResourceGroupName -TemplateFile $templateLocation -TemplateParameterFile $paramsLocation -Name "FeedbackBotBackEndDeployment" -Verbose

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


$scriptPath = Get-ScriptDirectory

# Load common script functions
$scriptContent = Get-Content -Path ($scriptPath + "\SharedFunctions.ps1") -Raw
Invoke-Expression $scriptContent


# Install the Az module if it's not already installed
$canInstall = LoadAzModuleGetAzContext
if ($canInstall) {
	ValidateAndInstall($ConfigFileName)
}
