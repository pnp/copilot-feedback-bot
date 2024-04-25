using Common.Engine;
using Common.Engine.Config;
using Common.Engine.Notifications;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Graph;

namespace Web.Bots;

public class FeedbackBot<T> : DialogueBot<T> where T : Dialog
{
    public readonly BotConfig _configuration;
    private readonly BotActionsHelper _helper;
    private readonly GraphServiceClient _graphServiceClient;
    BotConversationCache _conversationCache;
    private readonly IConversationResumeHandler _conversationResumeHandler;

    public FeedbackBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogueBot<T>> logger, BotActionsHelper helper, GraphServiceClient graphServiceClient,
        BotConfig configuration, BotConversationCache botConversationCache, IConversationResumeHandler conversationResumeHandler)
        : base(conversationState, userState, dialog, logger)
    {
        _helper = helper;
        _graphServiceClient = graphServiceClient;
        _conversationCache = botConversationCache;
        _conversationResumeHandler = conversationResumeHandler;
        _configuration = configuration;
    }

    /// <summary>
    /// New thread with bot. Only happens once per user in Teams.
    /// </summary>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                var userIdentity = await BotUserUtils.GetBotUserAsync(member, _configuration, _graphServiceClient);

                // Is this an Azure AD user?
                if (!userIdentity.IsAzureAdUserId)
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hi, anonynous user. I only work with Azure AD users in Teams normally..."));

                // Have we spoken before?
                await _conversationCache.PopulateMemCacheIfEmpty();
                var cachedUser = _conversationCache.GetCachedUser(userIdentity.UserId);
                if (cachedUser == null)
                {
                    // Add current user to conversation reference cache.
                    await _conversationCache.AddConversationReferenceToCache((Activity)turnContext.Activity, userIdentity);

                    // First time meeting a user (new thread). Can be because we've just installed the app. Introduce bot and start a new dialog.
                    await _helper.SendBotFirstIntro(turnContext, cancellationToken);
                }
                else
                {
                    // Resume conversation.
                    var upn = cachedUser.UserPrincipalName ?? _configuration.TestUPN;
                    if (upn != null)
                    {
                        // Send next survey card
                        var nextActionAndCard = await _conversationResumeHandler.GetProactiveConversationResumeConversationCard(upn);
                        var resumeActivity = MessageFactory.Attachment(nextActionAndCard.Item2);
                        await turnContext.SendActivityAsync(resumeActivity, cancellationToken);

                    }
                    else
                        await _helper.SendBotResumeConvo(turnContext, cancellationToken);
                }
            }
        }
    }

    protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

        // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.

        // Run the Dialog with the new Invoke Activity.
        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }
}
