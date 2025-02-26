
Param 
(
	$ConfigFileName = "InstallConfig.json"
)


function Get-ScriptDirectory {
	$Invocation = (Get-Variable MyInvocation -Scope 1).Value
	Split-Path $Invocation.MyCommand.Path
}
function Get-AcrNameArmTemplateValue {
	param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "acr_name"
}

$scriptPath = Get-ScriptDirectory

# Load common script functions
$scriptContent = Get-Content -Path ($scriptPath + "\SharedFunctions.ps1") -Raw
Invoke-Expression $scriptContent

$canInstall = LoadAzModuleGetAzContext
if ($canInstall) {
    WriteI -message "Connecting to ACR..."

    
	# Load config and sanity check
	try {
		$config = Get-Content ($scriptPath + "\" + $configFileName) -Raw -ErrorAction Stop | ConvertFrom-Json
		Write-Host ("Read configuration for '" + ($configFileName) + "'...")
	}
	catch {
		WriteE -message "FATAL ERROR: Cannot open config-file '$configFileName'"
		return
	}

	# Validate ARM params
	$parameters = Get-Content ($scriptPath + "\" + $config.ARMParametersFileBackend) -Raw -ErrorAction Stop | ConvertFrom-Json
	if ($null -eq $parameters) {
		WriteE -message "Error: ARM parameters file not found or valid JSon." 
		return
	}

	$acrName = Get-AcrNameArmTemplateValue $config

	WriteI -message "Connecting to ACR $acrName..."
    Connect-AzContainerRegistry -Name $acrName -erroraction Stop

	WriteI -message "Tagging and pushing images to ACR $acrName..."
	docker tag copilotbot-importer "$acrName.azurecr.io/copilotbot-importer"
	docker tag copilotbot-importer "$acrName.azurecr.io/copilotbot-functions"
	docker tag copilotbot-importer "$acrName.azurecr.io/copilotbot-web"

	docker push "$acrName.azurecr.io/copilotbot-importer"
	docker push "$acrName.azurecr.io/copilotbot-functions"
	docker push "$acrName.azurecr.io/copilotbot-web"
}