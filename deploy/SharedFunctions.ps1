
function Get-ArmTemplateValue {
    param (
        [parameter(mandatory = $true)] $config,
        [parameter(mandatory = $true)] $parameterName
    )

    if ($null -eq $config) {
        WriteE -message "Error: Configuration object is null." 
        return
    }
    if ($null -eq $config.ARMParametersFileAppServices) {
        # Write contents of $config
        Write-Host $config
        
        WriteE "Error: ARMParametersFileAppServices value is null."
        return
    }

    $parametersContent = Get-Content ($scriptPath + "\" + $config.ARMParametersFileAppServices) -Raw -ErrorAction Stop | ConvertFrom-Json

    return $parametersContent.parameters.$parameterName.value
}


function Get-AppServiceNameArmTemplateValue {
    param ( [parameter(mandatory = $true)] $config )
    return Get-ArmTemplateValue $config "app_service_name"
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

function LoadAzModuleGetAzContext() {
    WriteI "Checking for Az PowerShell module..."
    $azPsInstalled = LoadModuleGetAzContext -moduleName "Az" -loadContext $true
    if (-not $azPsInstalled) {
        WriteI -message "Documentation: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell"
    }
    return $azPsInstalled
}
function LoadModuleGetAzContext {
    param(
        [parameter(mandatory = $true)]
        [string]$moduleName,
        [parameter(mandatory = $true)]
        [bool]$loadContext
    )

    $moduleInstalled = $false
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        if (Get-InstalledModule -Name $moduleName -ErrorAction SilentlyContinue) {
            $moduleInstalled = $true
        }
    }
    else {
        if (Get-Module -ListAvailable -Name $moduleName) {
            $ver = Get-Module -ListAvailable -Name $moduleName | Select-Object -First 1 -ExpandProperty Version
            
            WriteI "Module $moduleName found with version $ver"
            $moduleInstalled = $true
        } 
    }

    if (-not $moduleInstalled) {
        WriteE -message "$moduleName PowerShell module not found. Please install it using 'Install-Module -Name $moduleName -AllowClobber -Scope CurrentUser'." 

        return $false
    }
    else {
        if ($loadContext) {
            WriteI -message "$moduleName PowerShell module found. Checking for Azure login context..."
	
            # Check if there is an Azure context
            $context = Get-AzContext

            if ($null -eq $context) {
                WriteE -message "No Azure login context found. Please log in using Connect-AzAccount."
                return $false
            }
            else {
                $accountId = $context.Account.Id
                WriteS -message "Azure login context found for account '$accountId'. Running script..."
                return $true
            }
        }
        else {
            WriteI -message "$moduleName PowerShell module found."
            return $true
        }
    }
}
