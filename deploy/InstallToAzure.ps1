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
function Get-FunctionAppServiceNameArmTemplateValue {
 param ( [parameter(mandatory = $true)] $config )
	return Get-ArmTemplateValue $config "function_app_service_name"
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

function Get-ArmTemplateValue {
	param (
		[parameter(mandatory = $true)] $config,
		[parameter(mandatory = $true)] $parameterName
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

	$solutionDeploySuccess = InstallAzComponents($config)
	if (-not $solutionDeploySuccess) {
		WriteE -message "Solution installation failed."
	}
	else {
		
		# Add the client's public IP to the SQL firewall
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

		WriteS -message "Solution installed successfully. Next steps:"
		WriteS -message "1. Verify import job is running in the web app and trace logs in Application Insights."
		WriteS -message "2. Check SQL tables have data once an import has finished."
	}
}

function AddClientPublicIpToSqlFirewall {
	param (
		[Parameter(Mandatory = $true)] $config
	)
	$ruleName = "SetupScript-$env:USERNAME-on-$env:COMPUTERNAME"
	$server = Get-SqlServerNameArmTemplateValue $config

	# Check if the rule already exists
	$existingRule = Get-AzSqlServerFirewallRule -ServerName $server -ResourceGroupName 'PsTests' | Where-Object { $_.FirewallRuleName -eq $ruleName}
	if ($null -ne $existingRule) {
		WriteW -message "Your public IP is already added to the SQL server firewall (rule name '$ruleName'), so we won't detect & add it again."
		return $true
	}

	# Get your current IP address
	WriteI -message "Getting your public IP address from http://checkip.dyndns.org..."
	$client = New-Object System.Net.WebClient

	try {
		[xml]$response = $client.DownloadString("http://checkip.dyndns.org")
	}
	catch {
		WriteE -message "Error: Unable to get your public IP address. This is likely because you're behind a proxy or firewall that blocks the request, or you have only an IPv6 address that can be seen externally."
		WriteW "You can always add your public IP address manually to the SQL server '$server' firewall using the Azure portal (rule name '$ruleName')."
		return $false
	}
	$ip = ($response.html.body -split ':')[1].Trim()

	WriteI -message "Adding your public IP '$ip' to the SQL server firewall (rule name '$ruleName')..."

	New-AzSqlServerFirewallRule -ServerName $server -ResourceGroupName $config.ResourceGroupName -FirewallRuleName $ruleName -StartIpAddress $ip -EndIpAddress $ip
	WriteS -message "Your public IP '$ip' has been added to the SQL server firewall (rule name '$ruleName')."
}
function AddUrlFiltersToDB {
	param (
		[Parameter(Mandatory = $true)] $config
	)
	$server = Get-SqlServerNameArmTemplateValue $config
	$database = Get-SqlDbNameArmTemplateValue $config
	$userId = Get-SqlServerUserNameArmTemplateValue $config
	$password = Get-SqlServerPasswordArmTemplateValue $config

	WriteI -message "Adding URL filters to database '$database' on server '$server' ..."

	$connectionString = "Server=tcp:$server.database.windows.net,1433;Initial Catalog=$database;Persist Security Info=False;User ID=$userId;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
	$connection = New-Object System.Data.SqlClient.SqlConnection
	$connection.ConnectionString = $connectionString

	$connection.Open()

	foreach ($site in $config.RecordAuditEventsOnSites) {
		$siteUrl = $site.SiteUrl
		$exactMatch = $site.ExactMatch

		$query = "
		IF NOT EXISTS (select * from [import_url_filter] where url_base = '$siteUrl')
		BEGIN
			INSERT INTO [import_url_filter] (url_base, exact_match) VALUES ('$siteUrl', '$exactMatch') 
		END
		" 

		$command = $connection.CreateCommand()
		$command.CommandText = $query

		$command.ExecuteNonQuery() | Out-Null
	}

	$connection.Close()
	WriteS -message "URL filters added to database '$database' on server '$server'."
}
function TriggerAppServiceWebJob {
	param (
		[Parameter(Mandatory = $true)] $config,
		[Parameter(Mandatory = $true)] $webAppName
	)
	WriteI -message "Triggering App Service webjob on app serving '$webAppName'..."
	$uri = "/subscriptions/$($config.SubcriptionId)/resourceGroups/$($config.ResourceGroupName)/providers/Microsoft.Web/sites/$webAppName/triggeredwebjobs/ActivityImporter.ConsoleApp/run?api-version=2023-12-01"
	$responseRaw = Invoke-AzRestMethod -Method POST -Path $uri `
		-Payload (@{ properties = @{ scmType = "None" } } | ConvertTo-Json) -Verbose

	if ($responseRaw.StatusCode -eq 200) {
		WriteS "Web-job triggered successfully."
	}
	else {
		if ($responseRaw.StatusCode -eq 404) {
			WriteW -message "App service webjob not found." 
		}
		else {
			WriteE -message "Error: start App service webjob failed. Response: $($responseRaw.Content)" 
		}
	}
}

function RemoveAppServiceDeployment {
	param (
		[Parameter(Mandatory = $true)] $config,
		$webAppName
	)
	$uri = "/subscriptions/$($config.SubcriptionId)/resourceGroups/$($config.ResourceGroupName)/providers/Microsoft.Web/sites/$webAppName/config/web?api-version=2023-12-01"
	$responseRaw = Invoke-AzRestMethod -Method PATCH -Path $uri `
		-Payload (@{ properties = @{ scmType = "None" } } | ConvertTo-Json) -Verbose

	if ($responseRaw.StatusCode -eq 200) {
		WriteS "App service deployment removed successfully."
	}
	else {
		if ($responseRaw.StatusCode -eq 404) {
			WriteW -message "App service deployment not found." 
		}
		else {
			WriteE -message "Error: App service deployment removal failed. Response: $($responseRaw.Content)" 
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

	# Remove the previous deployments so the ARM template can deploy the new ones
	RemoveAppServiceDeployment $config $webAppName	
	RemoveAppServiceDeployment $config $funcWebAppName
	
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
	if ($null -eq $config.ARMParametersFile) {
		WriteE "Error: ARMParametersFile is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
		return $false
	}

	if ($null -eq $config.RecordAuditEventsOnSites -or $config.RecordAuditEventsOnSites.Count -eq 0) {
		WriteE "Error: RecordAuditEventsOnSites is missing in the configuration file. See 'InstallConfig-template.json' for reference configuration file needed."
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