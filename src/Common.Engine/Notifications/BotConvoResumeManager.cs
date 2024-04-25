using Common.Engine.Config;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Common.Engine.Notifications;

public class BotConvoResumeManager : IBotConvoResumeManager
{
    private const string TeamsBotFrameworkChannelId = "msteams";

    private readonly ILogger _loggerBotConvoResumeManager;
    private readonly ILogger<BotAppInstallHelper> _loggerBotAppInstallHelper;
    private readonly BotConversationCache _botConversationCache;
    private readonly IConversationResumeHandler _conversationResumeHandler;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly TeamsAppConfig _config;
    private readonly IBotFrameworkHttpAdapter _adapter;

    public BotConvoResumeManager(ILogger<BotConvoResumeManager> loggerBotConvoResumeManager,
        ILogger<BotAppInstallHelper> loggerBotAppInstallHelper,
        BotConversationCache botConversationCache,
        IConversationResumeHandler conversationResumeHandler,
        GraphServiceClient graphServiceClient, TeamsAppConfig config, IBotFrameworkHttpAdapter adapter)
    {
        _loggerBotConvoResumeManager = loggerBotConvoResumeManager;
        _loggerBotAppInstallHelper = loggerBotAppInstallHelper;
        _botConversationCache = botConversationCache;
        _conversationResumeHandler = conversationResumeHandler;
        _graphServiceClient = graphServiceClient;
        _config = config;
        _adapter = adapter;
    }

    public async Task ResumeConversation(string upn)
    {
        // Get AAD user ID from Graph by looking up user by email
        User? graphUser = null;
        try
        {
            graphUser = await _graphServiceClient.Users[upn].GetAsync(op => op.QueryParameters.Select = ["Id"]);
        }
        catch (ODataError ex)
        {
            _loggerBotConvoResumeManager.LogWarning($"Couldn't get user by UPN '{upn}' - {ex.Message}");
        }
        if (graphUser?.Id != null)
        {
            // Do we have a conversation with this user yet?
            if (_botConversationCache.ContainsUserId(graphUser.Id))
            {
                var cachedUser = _botConversationCache.GetCachedUser(graphUser.Id)!;
                var convoId = cachedUser.ConversationId;

                var previousConversationReference = new ConversationReference()
                {
                    ChannelId = TeamsBotFrameworkChannelId,
                    Bot = new ChannelAccount() { Id = $"28:{_config.AppCatalogTeamAppId}" },
                    ServiceUrl = cachedUser.ServiceUrl,
                    Conversation = new ConversationAccount() { Id = cachedUser.ConversationId },
                };

                // Continue conversation with the registered "resume conversation" service
                var (nextCopilotEvent, surveyCard) = await _conversationResumeHandler.GetProactiveConversationResumeConversationCard(upn);

                var resumeActivity = MessageFactory.Attachment(surveyCard);

                await ((CloudAdapter)_adapter)
                    .ContinueConversationAsync(_config.AuthConfig.ClientId, previousConversationReference,
                    async (turnContext, cancellationToken) =>
                        await turnContext.SendActivityAsync(resumeActivity, cancellationToken), CancellationToken.None);
            }
            else
            {
                // No conversation with this user yet, so install the bot app for them
                if (string.IsNullOrEmpty(_config.AppCatalogTeamAppId))
                {
                    _loggerBotConvoResumeManager.LogError($"Can't install Teams app for bot - no {nameof(_config.AppCatalogTeamAppId)} found in configuration");
                }
                else
                {
                    var installManager = new BotAppInstallHelper(_loggerBotAppInstallHelper, _graphServiceClient);
                    try
                    {
                        // Install app and if already installed, trigger a new conversation update. This will then be picked up by the bot and the conversation ID then cached for this user.
                        await installManager.InstallBotForUser(graphUser.Id, _config.AppCatalogTeamAppId,
                            async () => await TriggerUserConversationUpdate(graphUser.Id, _config.AppCatalogTeamAppId, installManager));
                    }
                    catch (ODataError ex)
                    {
                        _loggerBotConvoResumeManager.LogWarning($"Couldn't install Teams app for user '{graphUser.Id}' - {ex.Message} - is user licensed for Teams?");
                    }
                }
            }
        }
    }

    async Task TriggerUserConversationUpdate(string userid, string appId, BotAppInstallHelper installManager)
    {
        _loggerBotConvoResumeManager.LogInformation($"Triggering new conversation with bot {appId} for user {userid}");

        // Docs here: https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages#-retrieve-the-conversation-chatid
        var installedApp = await installManager.GetUserInstalledApp(userid, appId);
        try
        {
            var chat = await _graphServiceClient.Users[userid].Teamwork.InstalledApps[installedApp.Id].Chat.GetAsync();
        }
        catch (ODataError ex)
        {
            _loggerBotConvoResumeManager.LogWarning($"Couldn't get chat for user '{userid}' - {ex.Message}");
        }
    }
}

public interface IBotConvoResumeManager
{
    public abstract Task ResumeConversation(string upn);
}
