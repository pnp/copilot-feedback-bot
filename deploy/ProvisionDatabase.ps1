#
# ProvisionDatabase.ps1
#
# Adds the filtering configuration to SQL using the provided configuration file. Without entries in import_url_filter, the importer will abort.

Param 
(
	$ConfigFileName = "InstallConfig.json", 	
	[switch] $UninstallOnly
)

function Get-ScriptDirectory {
	$Invocation = (Get-Variable MyInvocation -Scope 1).Value
	Split-Path $Invocation.MyCommand.Path
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

	$firewallConfigured = AddClientPublicIpToSqlFirewall $config
	
	# Make sure we have at least one site to filter, otherwise webjob won't run
	if ($firewallConfigured -eq $true) {
		# Add the URL filters to the database
		AddUrlFiltersToDB $config

		WriteS -message "Database provisioning completed successfully. All setup tasks have been completed."
	}
	else {
		WriteE -message "Error: Unable to add URL filters to the database. Please add manually to table 'import_url_filter'"
	}
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

function AddClientPublicIpToSqlFirewall {
	param (
		[Parameter(Mandatory = $true)] $config
	)
	$ruleName = "SetupScript-$env:USERNAME-on-$env:COMPUTERNAME"
	$server = Get-SqlServerNameArmTemplateValue $config

	# Check if the rule already exists
	$existingRule = Get-AzSqlServerFirewallRule -ServerName $server -ResourceGroupName $config.ResourceGroupName | Where-Object { $_.FirewallRuleName -eq $ruleName}
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


# Set global variables
$scriptPath = Get-ScriptDirectory


# Load common script functions
$scriptContent = Get-Content -Path ($scriptPath + "\SharedFunctions.ps1") -Raw
Invoke-Expression $scriptContent

# Check if we can install
$canInstall = LoadAzModuleGetAzContext
if ($canInstall) {
	ValidateAndInstall($ConfigFileName)
}