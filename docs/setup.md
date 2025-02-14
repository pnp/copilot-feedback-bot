
# Setup Steps
For this, we assume a decent knowledge of Teams apps deployment, .Net, and Azure PaaS. 

Note, that for all these steps you can do them all in PowerShell if you wish. 

You need Teams admin rights and rights to assign sensitive privileges for this setup to work. The bot app is just a bot that interacts with all/any Teams users - there's another Teams app for administering the solution that should be more restrictively installed.

## Create Bot with new App ID
1. Go to: https://dev.teams.microsoft.com/bots and create a new bot (or alternatively in the Azure Portal, create a new Azure bot - the 1st link doesn't require an Azure subscription).
2. Create a new client secret for the bot application registration. Note down the client ID & the secret of the bot.
3. Grant permissions (specified below) and have an admin grant consent.

## PowerShell Setup for Cloud Components (Backend)
There is a script to deploy all the Azure components and configure them. Recommended you use PowerShell 7 or above. 

1. Install Az PowerShell - https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell
2. Authenticate with your target Azure subscription with ```Connect-AzAccount```

Now we need to configure some parameters to create the backend, specifically the bot app details and the resource names we want in Azure. 
3. Copy script config file template ```deploy/InstallConfig-template.json``` to ```deploy/InstallConfig.json```
   1. Fill out mandatory values and check the others. This file contains where the ARM parameters file is, and some basic Azure subscription/resources config. 
4. Copy ARM parameters file template ```deploy/ARM/parameters-backend-template.json``` to ```deploy/ARM/parameters-backend.json```
   1. Fill out mandatory values and check the others. This is to install everything except the App Service. 
5. In the project root folder, run: ```deploy/InstallBackEnd.ps1```
6. Installation will take upto 45 mins if not run before.
7. You can run multiple times; if a resources is already created, it'll be skipped. 


## Configure App Registration API Access
Configure API access for app registration so the JavaScript app can access the backend. If you want Teams SSO to work, the URL of the JavaScript must match the App URI too. If you don't care, then you can pick any URI. 
1. The application URI needs to be in format: api://[DOMAIN]/[CLIENT_ID], with port if not standard. Examples:
   1. api://contosobot.azurewebsites.net/5023a8dc-8448-4f41-b34c-131ee03def2f
   2. api://localhost:5173/c8c85903-7e4a-4314-898b-08d01382e025
      This value is needed for the ```AuthConfig__ApiAudience``` configuration.
2. Add a scope for users/admins - "access".
3. Copy the full scope name for the ARM template parameters ```api_scope``` value - "api://contosobot.azurewebsites.net/5023a8dc-8448-4f41-b34c-131ee03def2f/access"

## Deploy User Teams App
Next, create a Teams app from the template to enable the bot in your org:
4. In ``Teams Apps`` root dir, copy file "manifest-template.json" to "manifest.json".
5. Edit ``manifest.json`` and update all instances of ```<<BOT_APP_ID>>``` with your app registration client ID. 
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
   1. copilotbot-functions - Azure functions app image.
   2. copilotbot-web - this contains the ASP.Net + built JavaScript with your bot registration details compiled in via the docker-compose.override.yml changes you made earlier.
* Next, we need to push them to your ACR that was created with the backend components. 

## Deploy Docker Images to Azure Container Registry
* Push image to your created Azure Container Registry. First you need to connect to it with:
```PowerShell
Connect-AzAccount # You might not need to do this
Connect-AzContainerRegistry -Name [myregistry]    # Change for your ACR name
```
* Tag images for ACR with:
  * docker tag copilotbot-functions [myregistry].azurecr.io/copilotbot-functions
  * docker tag copilotbot-web [myregistry].azurecr.io/copilotbot-web
* Push images with:
  * docker push [myregistry].azurecr.io/copilotbot-functions
  * docker push [myregistry].azurecr.io/copilotbot-web

## PowerShell Setup for Cloud Components (Compute)
Now with the backend created and the docker images pushed to an ACR, we can deploy the compute components in Azure App Services.

1. Copy ARM parameters file template ```deploy/ARM/parameters-appservices-template.json``` to ```deploy/ARM/parameters-appservices.json```
   1. Fill out mandatory values and check the others. This is to install the App Services and Application Insights. 
   2. **Very important**: make sure "imageWebServer" and "imageFunctions" parameters both follow this format:
        ```DOCKER|[myregistry].azurecr.io/copilotbot-functions:[tag]```
      For example:
        ```DOCKER|contosofeedbackbotprod.azurecr.io/copilotbot-functions:latest```
      If the value isn't 100%, the app service deployment will fail.

2. In the project root folder, run: ```deploy/InstallAppService.ps1```.
3. The script will deploy an app-service and functions app using the image names you've built and published. 
4. Check the app-service deployment center logs to make sure everything deployed ok.

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

_Note_: For Docker/Linux, values with ":" in the configuration should be replaced with double underscore "__" as ":" is not supported for environmental variable names.
Example: ```ConnectionStrings__SQL``` instead of ```ConnectionStrings:SQL```.

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