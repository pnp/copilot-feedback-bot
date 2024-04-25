# Copilot Feedback Bot
Get real feedback from your organisations users about how copilot is helping, with "Copilot Feedback Bot".

![Copilot Feedback Bot says 'Hiii!'](imgs/bot-salute-small.png)

This is a bot that collects user feedback about copilot use within M365 so you can see exactly what your users think about it. Do your users love copilot? Does it really save them time? Doing what actions? Do some groups of people like it more than others?

More importantly: is it worth the investment? This is the question this system seeks to quantify in more detail than anything else. 

![User Satisfaction Dashboard](imgs/report1.png)

Copilot events are detected automatically, and each user that used copilot will be surveyed about how that interaction went through a Teams bot. 

![Teams Prompt from Bot](imgs/botconvo.jpg)

User responses are stored in a database and visualised in Power BI.

![User Satisfaction Report](imgs/report2.png)
This is an automated way to get real feedback from your users about what they think of copilot in Office 365. 

All data is stored in your own SQL Server.

## Usage
**Quick version**: run the activity import and let the bot do the rest. The functions app will look for users that have used copilot and then survey them.

When deployed, a web-job will read the Office 365 Activity API to determine copilot interactions - whom, and what. It runs automatically once a day. 

A functions app will then find users that have new activity and start a new conversation with them to ask if they could review the activity in question with copilot. 

Once surveyed they won't be surveyed again for that specific interaction (even if they don't answer). 

If the user doesn't have the bot installed in Teams already, it'll be installed automatically.

If the user says anything to the bot outside the normal dialogue flow, the assumption is they want to leave a copilot review. Surveys don't necessarily have to correlate to a specific interaction, the user can just leave general feedback too. 

### Testing the Bot
Copilot activity can be faked by sending a **POST** to ```/api/Triggers/GenerateFakeActivityFor?upn={**UPN**}``` (no body required). This will create a fake meeting & file activity for that user.

The bot can also be forced to check and send for pending surveys to be sent everyone with a **POST** to ```/api/Triggers/SendSurveys``` (no body required).

To test-install the bot for a user and force a "hi!" from the bot, independently of if they have pending surveys or not, **POST** to ```/api/Triggers/InstallBotForUser?upn={**UPN**}``` (no body required).

Otherwise, the longer version is to use copilot in a file or team meeting, wait a few minutes, run (manually or automatically) the import job and then wait for the timer function to run the "_FindAndProcessNewSurveyEventsAllUsers_" method.

## How Does it Work?
Easy. There's a web-job/process that reads O365 audit events for SharePoint and copilot activity. We read SharePoint to know what happened with files (edit/view/etc) and the copilot to know what users have done with copilot. The information we get from copilot events is limited; "User A chatted to copilot in an app, and it involved this file/meeting", so hence we read SP logs too to link what actually was going on.

    Note: SharePoint events that are saved can be filtered so we don't accidentally store sensitive file activity data in table "import_url_filter".

In addition we also read aggregated activity reports for users for OneDrive, SharePoint, Outlook and Teams. That way we can find users that are active in O365 but not using Copilot. 

Finally the solution reads user metadata from Entra ID (Azure AD) copilot activity & satisfaction by demographics can be analysed - job title, office location, etc. 

# Setup Steps
For this, we assume a decent knowledge of Teams apps deployment, .Net, and Azure PaaS. 

## Create Bot and Deploy Teams App
Note, that for all these steps you can do them all in PowerShell if you wish. I’m not a sysadmin so this is what works best for me. 

You need Teams admin rights and rights to assign sensitive privileges for this setup to work. It's a bot that proactively installs itself and asks users. 

1. Go to: https://dev.teams.microsoft.com/bots and create a new bot (or alternatively in the Azure Portal, create a new Azure bot - the 1st link doesn't require an Azure subscription).
2. Create a new client secret for the bot application registration. Note down the client ID & the secret of the bot.
3. Grant permissions (specified below) and have an admin grant consent.
4. Optional: you can create another app registration for the more sensitive privileges if you need (service account). The config supports having x2 accounts, but one can be used for both. 

Next, create a Teams app from the template:
4. In "Teams App" root dir, copy file "manifest-template.json" to "manifest.json".
5. Edit "manifest.json" and update all instances of ```<<BOT_APP_ID>>``` with your app registration client ID. 
6. Make a zip file from "color.png", "manifest.json", and "outline.png" files in that folder. Make sure all files are in the root of the zip. 
7. Deploy that zip file to your apps catalog in Teams admin.
8. Once deployed, copy the "App ID" generated. We'll need that ID for bot configuration.

## Create Azure Resources
Create these resources:

* Required - App service & plan. B1 level recommended for small environments. 
* Required - Functions app - consumption plan recommended. May require it's own resource-group.
* Application Insights (link app service & functions app to it)
* Required - Storage account.
* Required - Service bus namespace.
* SQL Database (and server) - the PaaS version. You could use an on-premises SQL if you wanted. 

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

## Deploy Solution
There are GitHub actions in ".github\workflows\" that will build and deploy the app service & webjob in one WF and the functions app in another. 

The workflows require secrets "feedbackbot_PUBLISH_PROFILE" and "feedbackbot_AZURE_FUNCTIONS_NAME" for deploy to work. 

Quick hack: publish from Visual Studio is also a (temporary) solution. 
