
# Setup Steps
For this, we assume a decent knowledge of Teams apps deployment, .Net, and Azure PaaS. 

## Create Bot and Deploy Teams Bot App
Note, that for all these steps you can do them all in PowerShell if you wish. 

You need Teams admin rights and rights to assign sensitive privileges for this setup to work. The bot app is just a bot that interacts with all/any Teams users - there's another Teams app for administering the solution that should be more restrictively installed.

### Create Bot
1. Go to: https://dev.teams.microsoft.com/bots and create a new bot (or alternatively in the Azure Portal, create a new Azure bot - the 1st link doesn't require an Azure subscription).
2. Create a new client secret for the bot application registration. Note down the client ID & the secret of the bot.
3. Grant permissions (specified below) and have an admin grant consent.

### Configure App Registration API Access
1. Configure API access for app registration so the JavaScript app can access the backend.
2. The application URI needs to be in format: api://[DOMAIN]/5023a8dc-8448-4f41-b34c-131ee03def2f, with port if not standard. Examples
   1. api://contosobot.azurewebsites.net/5023a8dc-8448-4f41-b34c-131ee03def2f
   2. api://localhost:5173/c8c85903-7e4a-4314-898b-08d01382e025
3. Add a scope for users/admins - "access".
4. Copy the full scope name for the ARM template parameters 'api_scope' value - "api://contosobot.azurewebsites.net/5023a8dc-8448-4f41-b34c-131ee03def2f/access"


### Deploy User Teams App
Next, create a Teams app from the template:
4. In ``Teams Apps`` root dir, copy file "manifest-template.json" to "manifest.json".
5. Edit ``manifest.json`` and update all instances of ```<<BOT_APP_ID>>``` with your app registration client ID. 
6. Make zip-file of the folder with files: ``manifest.json``, ``color.png``, ``outline.png`` only. Make sure zip file has these files in the root.  
7. Deploy that zip file to your apps catalog in Teams admin.
8. Once deployed, copy the "App ID" generated. We'll need that ID for bot configuration so the bot can self-install to users that don't have it yet, and then send proactive messages.

## PowerShell Setup for Cloud Components (Backend)
There is a script to deploy all the Azure components and configure them. Recommended you use PowerShell 7 or above. 

1. Install Az PowerShell - https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell
2. Authenticate with your target Azure subscription with ```Connect-AzAccount```
3. Copy script config file template ```deploy/InstallConfig-template.json``` to ```deploy/InstallConfig.json```
   1. Fill out mandatory values and check the others.
4. Copy ARM parameters file template ```deploy/ARM/parameters-template.json``` to ```deploy/ARM/parameters.json```
   1. Fill out mandatory values and check the others.
5. In the project root folder, run: ```deploy/InstallToAzure.ps1```
6. Installation will take upto 45 mins if not run before.
7. You can run multiple times; if a resources is already created, it'll be skipped. 


## Build Docker Image
To deploy the bot for production, we use docker to build a new bot image with the ASP.Net + JavaScript application in a single image.
* Copy ``src\docker-compose.override - template.yml`` to ``src\docker-compose.override.yml``.
* Fill out fields in override file. You'll need your Azure PaaS resource details + the bot app registration configuration. 
* Build image with ``docker-compose build``.

## Deploy Docker Images
* Push image to your created/existing ACR.
* ...tbd... 

## PowerShell Setup for Cloud Components (Compute)
Now with the backend created, the docker images pushed to an ACR, we can deploy the compute components in Azure App Services.
...tbd...

## Manual Setup - Create Azure Resources
You can deploy the ARM template manually:

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpnp%2Fcopilot-feedback-bot%2Fmain%2Fdeploy%2FARM%2Ftemplate.json)

Template is at ```deploy/ARM/template.json``` in this repository if you wish to copy/paste into the portal manually. 

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
ConnectionStrings:Redis | Redis connection string, used for caching delta tokens
ConnectionStrings:ServiceBusRoot | Used for async processing of various things
ConnectionStrings:SQL | The database connection string.
ConnectionStrings:Storage | Connection string. Conversation cache and other table storage

ConnectionStrings go in their own section in App Services (and prefix "ConnectionStrings:" to the name you give there for .Net). 

_Important:_ **If any values are missing, the process will crash at start-up**. Check the local VM application log if you get a start-up error (in Kudu for app services).

## Application Permissions
Graph permissions needed (application):
* User.Read.All - for looking up user metadata, allowing activity & survey slicing by demographics (job title, location etc)
* Reports.Read.All - for reading activity data so we can cross-check who's active but not using copilot. 
* TeamsAppInstallation.ReadWriteForUser.All - so the bot can proactively install itself into users Teams, to start a new conversation. 

Office 365 Management APIs
* ActivityFeed.Read - for detecting copilot events. 

All these permissions need administrator consent to be effective. 

## Optional - Deploy Bot Admin Teams App
There is a React web application deployed to the app service that handles administration of bot questions, and other areas. The app can be accessed via MSAL logins or with Teams SSO. Teams is the preferred method as it doesn't require any extra authentication. 

These steps require the Azure app service to exist. 

* Modify bot app registration for for single-page application App registration for MSAL 2.0 - https://learn.microsoft.com/en-us/entra/identity-platform/scenario-spa-app-registration#redirect-uri-msaljs-20-with-auth-code-flow

If you want to embed the admin app in Teams to leverage the single-sign-on:
* Configure SSO for bot app registration - https://learn.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/tab-sso-register-aad
* Create Teams Admin app:
  * In ``Teams Apps/Admin`` copy ``manifest-template.json`` to ``manifest.json``.
  * In ``manifest.json``, replace values: ``<<BOT_APP_ID>>``, ``<<WEB_DOMAIN>>``, ``<<WEB_HTTPS_ROOT>>``
    * Examples: ``5023a8dc-8448-4f41-b34c-131ee03def2f``, ``contosofeedbackbot.azurewebsites.net``, ``https://contosofeedbackbot.azurewebsites.net``
    * Note: for localhost testing, the ``<<WEB_DOMAIN>>`` value must include the port if non-standard. Example: ``localhost:5173``
  * Make zip-file of the folder with files: ``manifest.json``, ``color.png``, ``outline.png`` only. Make sure zip file has these files in the root. 
  * Upload zip-file to Teams admin centre and publish application to admin users/groups. _Careful who you give access!_

## Alternative: Deploy Solution via GitHub Actions
There are GitHub actions in ".github\workflows\" that will build and deploy the app service & webjob in one WF and the functions app in another. 

The workflows require secrets "feedbackbot_PUBLISH_PROFILE" and "feedbackbot_AZURE_FUNCTIONS_NAME" for deploy to work. 