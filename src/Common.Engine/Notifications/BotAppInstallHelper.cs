using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Net;

namespace Common.Engine.Notifications;

/// <summary>
/// Bot functionality
/// </summary>
public class BotAppInstallHelper
{
    #region Privates & Constructors

    private readonly ILogger<BotAppInstallHelper> _logger;
    private readonly GraphServiceClient _graphServiceClient;

    public BotAppInstallHelper(ILogger<BotAppInstallHelper> logger, GraphServiceClient graphServiceClient)
    {
        _logger = logger;
        _graphServiceClient = graphServiceClient;
    }

    #endregion

    /// <summary>
    /// Install the app so we can get a conversation reference. 
    /// </summary>
    public async Task InstallBotForUser(string userid, string teamAppid, Func<Task> conflictCallback)
    {
        _logger.LogInformation($"Installing app with ID {teamAppid} for user {userid}");

        try
        {
            var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/" + teamAppid}
                }
            };

            await _graphServiceClient.Users[userid].Teamwork.InstalledApps
                .PostAsync(userScopeTeamsAppInstallation);
        }
        catch (ODataError odataError)
        {
            // This is where app is already installed but we don't have conversation reference.
            if (odataError.ResponseStatusCode == (int)HttpStatusCode.Conflict)
            {
                _logger.LogInformation($"User {userid} already has app with ID {teamAppid}");
                await conflictCallback();
            }
            else if (odataError.ResponseStatusCode == (int)HttpStatusCode.BadRequest)
            {
                throw new Exception($"Can't install for user - is user licensed?");
            }
            else if (odataError.ResponseStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new Exception($"Teams app ID '{teamAppid}' doesn't seem to exist");
            }
            else throw;
        }
    }

    public async Task UninstallBotForUser(string userid, string appId)
    {
        UserScopeTeamsAppInstallation? installedApp = null;
        _logger.LogTrace($"Uninstalling app with ID {appId} for user {userid}");

        try
        {
            installedApp = await GetUserInstalledApp(userid, appId);
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        if (installedApp != null)
            await _graphServiceClient.Users[userid].Teamwork.InstalledApps[installedApp.Id]
                    .DeleteAsync();
    }

    public async Task<UserScopeTeamsAppInstallation> GetUserInstalledApp(string userid, string appId)
    {
        // Docs here: https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages#-retrieve-the-conversation-chatid
        var installedApps = await _graphServiceClient.Users[userid].Teamwork.InstalledApps
            .GetAsync(ops => { ops.QueryParameters.Filter = $"teamsAppDefinition/teamsAppId eq '{appId}'"; ops.QueryParameters.Expand = ["teamsAppDefinition"]; });

        var a = installedApps?.Value?.FirstOrDefault();
        if (a == null)
        {
            var msg = $"Can't find Teams app with id {appId} for user {userid}";
            _logger.LogWarning(msg);
            throw new ArgumentOutOfRangeException(nameof(appId), msg);
        }

        return a;
    }
}
