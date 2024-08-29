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

# Install custom action in all sites listed in the config
function ProcessScriptWithConfig ($configFileName) {

	# Load config and sanity check
	try {
		$config = Get-Content ($scriptPath + "\" + $configFileName) -Raw -ErrorAction Stop | ConvertFrom-Json
		Write-Host ("Read configuration for environment name '" + ($config.EnvironmentName) + "'...")
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
		Write-Host "Resource group '"($config.ResourceGroupName) "' created in location '" + $config.ResourceGroupLocation + "'." -ForegroundColor Yellow
	}
 else {
		Write-Host "Resource group '"($config.ResourceGroupName)"' already exists." -ForegroundColor Green
	}

	InstallAzComponents($config)
}

function Get-AppServiceNameArmTemplateValue {
	param (
		$config
	)
	return Get-ArmTemplateValue $config "app_service_name"
}
function Get-ArmTemplateValue {
	param (
		$config,
		$parameterName
	)

	$parametersContent = Get-Content ($scriptPath + "\" + $config.ARMParametersFile) -Raw -ErrorAction Stop | ConvertFrom-Json

	return $parametersContent.parameters.$parameterName.value
}

function InstallAzComponents {
	param (
		$config
	)

	# Variables
	$templateFilePath = "ARM\template.json"
	$deploymentName = "FeedbackBotDeployment"

	# Deploy the ARM template
	Write-Host "Deploying ARM template..." -ForegroundColor Yellow
	$armDeploy = New-AzResourceGroupDeployment -ResourceGroupName $config.ResourceGroupName -TemplateFile $templateFilePath -TemplateParameterFile $config.ARMParametersFile -Name $deploymentName -Verbose
	if ($armDeploy.ProvisioningState -eq "Succeeded") {
		Write-Host "ARM template deployment succeeded." -ForegroundColor Green
	}
	else {
		Write-Host "ARM template deployment failed. See resource-group deployment tab for details." -ForegroundColor Red
		return
	}


	# Create a WebJob
	$webJobProperties = @{
		name       = $webJobName
		type       = "continuous"
		runCommand = "run.cmd"
	}

	$webJobFileContent = Get-Content -Path $webJobFilePath -Raw
	$webJobEncodedContent = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($webJobFileContent))

	$webJobPayload = @{
		properties = $webJobProperties
		file       = $webJobEncodedContent
	}

	Invoke-RestMethod -Uri "https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/" + $config.ResourceGroupName + "/providers/Microsoft.Web/sites/$webAppName/webjobs/$webJobName?api-version=2018-02-01" -Method Put -Body ($webJobPayload | ConvertTo-Json) -ContentType "application/json" -Headers @{
		Authorization = "Bearer $(Get-AzAccessToken -ResourceUrl https://management.azure.com/).Token"
	}
}

function ValidateConfig {
	param (
		$config
	)

	if ($config.EnvironmentName -eq $null) {
		Write-Host "Error: EnvironmentName is missing in the configuration file." -ForegroundColor Red
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
# Install the things
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
		
		ProcessScriptWithConfig($ConfigFileName)
	}
}