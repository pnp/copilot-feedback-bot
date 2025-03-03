
Param 
(
	$ConfigFileName = "InstallConfig.json"
)


function Get-ScriptDirectory {
	$Invocation = (Get-Variable MyInvocation -Scope 1).Value
	Split-Path $Invocation.MyCommand.Path
}
$scriptPath = Get-ScriptDirectory

# Load common script functions
$scriptContent = Get-Content -Path ($scriptPath + "\SharedFunctions.ps1") -Raw
Invoke-Expression $scriptContent

$canInstall = LoadAzModuleGetAzContext
if ($canInstall) {

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
	docker tag copilotbot-functions "$acrName.azurecr.io/copilotbot-functions"
	docker tag copilotbot-web "$acrName.azurecr.io/copilotbot-web"

	docker push "$acrName.azurecr.io/copilotbot-importer"
	docker push "$acrName.azurecr.io/copilotbot-functions"
	docker push "$acrName.azurecr.io/copilotbot-web"

	WriteS -message "Published to your Azure Container Registry. Next steps:"
	WriteS -message "1. Check for any errors above."
	WriteS -message "2. Run the InstallAppService.ps1 script."

	Write-Host ""
	WriteS -message "Edit '$($config.ARMParametersFileAppServices)' and set these ARM template parameters in the JSON:"
	WriteI -message "`"imageWebServer`": 	{ `"value`": `"$($acrName).azurecr.io/copilotbot-web:latest`"		},"
	WriteI -message "`"imageFunctions`": 	{ `"value`": `"$($acrName).azurecr.io/copilotbot-functions:latest`"	},"
	WriteI -message "`"imageImporter`": 	{ `"value`": `"$($acrName).azurecr.io/copilotbot-importer:latest`"	},"
}