
# Setup Steps
For this, we assume a decent knowledge of Teams apps deployment, .Net, and Azure PaaS. 

Note, that for all these steps you can do them all in PowerShell if you wish. 

You need Teams admin rights and rights to assign sensitive privileges for this setup to work. The bot app is just a bot that interacts with all/any Teams users - there's another Teams app for administering the solution that should be more restrictively installed.

**High-level setup guide:**
1. Create a bot + app ID. Configure secret + assign Graph access.
2. Deploy user Teams app to Teams for bot - we need the installed app ID for configuring the bot later. 
3. Deploy Azure back-end components.
4. Build a docker images for bot + JavaScript application, and functions app. Requires configuration info from bot app.
5. Push image to Azure container registry created in step 3.
6. Deploy compute components to Container Apps Environment with published docker images.
7. Verify deployment.

Docker is used in this guide just because it gives a consistent build process, but is not essential. You could also build and publish with GitHub actions. 

## Requirements
Check the [prerequisites](prereqs.md) document before attempting setup. 

## Create Bot with new App ID
1. Go to: https://dev.teams.microsoft.com/bots and create a new bot (or alternatively in the Azure Portal, create a new Azure bot - the 1st link doesn't require an Azure subscription).
2. Create a new client secret for the bot application registration. Note down the client ID & the secret of the bot.
3. Grant permissions (specified below) and have an admin grant consent.

## PowerShell Setup for Backend in Azure
There is a script to deploy all the Azure components and configure them. Recommended you use PowerShell 7 or above. 

1. Install Az PowerShell - https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell
2. Authenticate with your target Azure subscription with ``Connect-AzAccount``

Now we need to configure some parameters to create the backend, specifically the bot app details and the resource names we want in Azure. 
3. Copy script config file template ``deploy/InstallConfig-template.json`` to ``deploy/InstallConfig.json``
   1. Fill out mandatory values and check the others. This file contains where the ARM parameters file is, and some basic Azure subscription/resources config. 
4. Copy ARM parameters file template ``deploy/ARM/parameters-backend-template.json`` to ``deploy/ARM/parameters-backend.json``
   1. Fill out mandatory values and check the others. This is to install everything except the compute components. 
5. In the project root folder, run: ``deploy/InstallBackEnd.ps1``
6. Installation will take upto 45 mins if not run before.
7. You can run multiple times; if a resources is already created, it'll be skipped.

## Configure App Registration 
There are a couple of extra configurations we need for the bot app. We'll take [PUBLIC_URL_DOMAIN] from the FQDN from the Azure Front Door created in the backend setup.

### API Access + SSO for Teams
Configure API access for app registration so the JavaScript app can access the backend. If you want Teams SSO to work, the URL of the JavaScript must match the App URI too. If you don't care, then you can pick any URI. 
1. The application URI needs to be in format: api://[PUBLIC_URL_DOMAIN]/[CLIENT_ID], with port if not standard. Examples:
   1. ``api://contosobot.azurefd.net/5023a8dc-8448-4f41-b34c-131ee03def2f``
   2. ``api://localhost:5173/c8c85903-7e4a-4314-898b-08d01382e025``
      This value is needed for the ``AuthConfig__ApiAudience`` configuration.
2. Add a scope for users/admins - ``access``.
3. Copy the full scope name into the ARM template parameters ``parameters-compute.json`` file: ``api_audience`` value - 
``api://[contosobot].azurefd.net/[5023a8dc-8448-4f41-b34c-131ee03def2f]/access``

### Add Reply URLs
For the JavaScript app to work, the reply URLs must be configured. 
1. Add reply URLs for a Single-page application - ``https://[PUBLIC_URL_DOMAIN]/``
2. Enable access tokens and ID tokens. 

## Deploy User Teams App
Next, create a Teams app from the template to enable the bot in your org:
4. In ``Teams Apps`` root dir, copy file "manifest-template.json" to "manifest.json".
5. Edit ``manifest.json`` and update all instances of ``<<BOT_APP_ID>>`` with your app registration client ID. 
6. Make zip-file of the folder with files: ``manifest.json``, ``color.png``, ``outline.png`` only. Make sure zip file has these files in the root.  
7. Deploy that zip file to your apps catalog in Teams admin.
8. Once deployed, copy the "App ID" generated. We'll need that ID for bot configuration so the bot can self-install to users that don't have it yet, and then send proactive messages.

## Build Docker Images
To deploy the bot for production, we use docker to build a new bot image with the ASP.Net + JavaScript application in a single image, and the functions app in another image.
* Copy ``src\docker-compose.override - template.yml`` to ``src\docker-compose.override.yml``.
  * Fill out all the build args for the copilotbot-web service - environmental data will be handled by Azure. Config will include:
    * Client ID of bot (VITE_MSAL_CLIENT_ID).
    * Optional for Teams SSO - login page URL (VITE_TEAMSFX_START_LOGIN_PAGE_URL.) There is a certain chicken/egg game here; we assume an app-service URL before we know we have it.
    * Created scope ID (VITE_MSAL_SCOPES), based on domain-name of app-service if Teams SSO is required, otherwise any ID.
* Build images with ``docker-compose build`` from the ``src`` folder (change directory to ".\src" to run command).
* You will end up with two images:
   1. ``copilotbot-functions`` - Azure functions app image.
   2. ``copilotbot-web`` - this contains the ASP.Net + built JavaScript with your bot registration details compiled in via the docker-compose.override.yml changes you made earlier.
   3. ``copilotbot-importer`` - an importer task that looks for usage activity. 
* Next, we need to push them to your ACR that was created with the backend components. 

## Deploy Docker Images to Azure Container Registry
Run the ``deploy/TagAndPushImages.ps1`` script to perform these steps automatically. It'll work if your ACR name in ``parameters-backend.json`` match the ACR created in the create back-end step.

## PowerShell Deploy for Cloud Components (Compute)
Now with the backend created and the docker images pushed to an ACR, we can deploy the compute components in an Azure Container Apps Environment.

1. Copy ARM parameters file template ``deploy/ARM/parameters-compute-template.json`` to ``deploy/ARM/parameters-appservices.json``
   1. Fill out mandatory values and check the others. This is to install the compute components and Application Insights. 
   2. **Important**: make sure "imageWebServer", "imageImporter" and "imageFunctions" parameters both follow this format:
        ``[myregistry].azurecr.io/copilotbot-functions:[tag]``
      For example:
        ``contosofeedbackbotprod.azurecr.io/copilotbot-functions:latest``
      If the value isn't 100%, the Container Apps deployment will fail.

2. In the project root folder, run: ``deploy/InstallCompute.ps1``.
The script will deploy a Container Apps Environment + Functions container app, a container app for the web, and a Container App Job for importer. 

### Update Azure Front Door
Once the compute PS is finished, the PowerShell will give you the randomly generated domain-name created for the web component which needs to be set in the Azure Front Door backend pool. Note: once configured, it may take a few minutes before the health probes realise the new web backend is healthy and therefore work via the AFD public URL. 

## Provision Database
The last step requires some SQL scripts and data to be inserted.

Run ``deploy/ProvisionDatabase.ps1`` to complete all SQL tasks and insert the URLs stored in your ``InstallConfig.json`` file into the import_url_filter table. 

## Recommended: Run Importer 1st Time
The first time the importer runs, it'll enable audit-logs subscriptions in O365 so data can be recorded. The sooner the importer does this, the sooner data will be available. The importer runs nightly at midnight by default, but you can also start it manually from the Azure Portal, under the Container App Job. 

### Manual Setup: Add URLs to import_url_filter Table
Without data in import_url_filter the importer will fail with error:

```
FATAL ERROR: No import URLs found in database! This means everything would be ignored for SharePoint audit data. Add at least one URL to the import_url_filter table for this to work.
```
This is a safety mechanism to make sure confidential activity does not accidentally end up in the SQL database. The system only reports on activity in general terms ("File X was edited by user Y"), and we recommend you add a broad a scope as possible, but this is a step you the customer must do yourself.

### Manual Setup: Run SQL Scripts for Profiling Extensions
If you can't run the PowerShell script above, you also need to execute all SQL scripts in ``deploy\profiling\CreateSchema``.

## Manual Setup - Create Back-end Azure Resources
You can deploy the ARM templates manually if you prefer. 

Backend components:

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpnp%2Fcopilot-feedback-bot%2Fmain%2Fdeploy%2FARM%2Ftemplate-backend.json)

Compute components:

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpnp%2Fcopilot-feedback-bot%2Fmain%2Fdeploy%2FARM%2Ftemplate-compute.json)

Templates are ``deploy/ARM/template-backend.json`` and ``deploy/ARM/template-compute.json`` in this repository if you wish to copy/paste into the portal manually. 

## Required Configuration 
These configuration settings are needed in the app service & functions app:

Name | Description
--------------- | -----------
AppCatalogTeamAppId | Teams app ID once deployed to the internal catalog
MicrosoftAppId | ID of bot Azure AD application
MicrosoftAppPassword | Bot app secret
WebAppURL | Root URL of app service
AuthConfig:ClientId | Service account 
AuthConfig:ClientSecret | Service account 
AuthConfig:TenantId | Service account 
AuthConfig:ApiAudience | API URI
AuthConfig:Authority | MSAL authority - usually https://login.microsoftonline.com/organizations
ConnectionStrings:Redis | Redis connection string, used for caching delta tokens
ConnectionStrings:ServiceBusRoot | Used for async processing of various things
ConnectionStrings:SQL | The database connection string.
ConnectionStrings:Storage | Connection string. Conversation cache and other table storage

_Note_: For Docker/Linux, values with ":" in the configuration should be replaced with double underscore "__" as ":" is not supported for environmental variable names.
Example: ``ConnectionStrings__SQL`` instead of ``ConnectionStrings:SQL``.

ConnectionStrings go in their own section in App Services (and prefix "ConnectionStrings:" to the name you give there for .Net). 

_Important:_ **If any values are missing, the process will crash at start-up**. Check the local VM application log if you get a start-up error (in Kudu for app services).

## Optional - Deploy Bot Admin Teams App
There is a React web application deployed to the app service that handles administration of bot questions, and other areas. The app can be accessed via MSAL logins or with Teams SSO. Teams is the preferred method as it doesn't require any extra authentication. 

These steps require the Azure app service to exist. 

* Modify bot app registration for for single-page application App registration for MSAL 2.0 - https://learn.microsoft.com/en-us/entra/identity-platform/scenario-spa-app-registration#redirect-uri-msaljs-20-with-auth-code-flow

If you want to embed the admin app in Teams to leverage the single-sign-on:
* Configure SSO for bot app registration - https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/tab-sso-register-aad
* Create Teams Admin app:
  * In ``Teams Apps/Admin`` copy ``manifest-template.json`` to ``manifest.json``.
  * In ``manifest.json``, replace values: ``<<BOT_APP_ID>>``, ``<<WEB_PUBLIC_URL_DOMAIN>>``, ``<<WEB_HTTPS_ROOT>>``
    * Examples: ``5023a8dc-8448-4f41-b34c-131ee03def2f``, ``contosofeedbackbot.azurefd.net``, ``https://contosofeedbackbot.azurefd.net``
    * Note: for localhost testing, the ``<<WEB_PUBLIC_URL_DOMAIN>>`` value must include the port if non-standard. Example: ``localhost:5173``
  * Make zip-file of the folder with files: ``manifest.json``, ``color.png``, ``outline.png`` only. Make sure zip file has these files in the root. 
  * Upload zip-file to Teams admin centre and publish application to admin users/groups. _Careful who you give access!_

## Alternative: Deploy Solution via GitHub Actions
There are GitHub actions in ``.github\workflows\`` that will build and deploy the app service & webjob in one WF and the functions app in another. 

The workflows require secrets ``feedbackbot_PUBLISH_PROFILE`` and ``feedbackbot_AZURE_FUNCTIONS_NAME`` for deploy to work. 
