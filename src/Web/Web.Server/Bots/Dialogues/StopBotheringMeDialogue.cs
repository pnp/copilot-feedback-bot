using Common.Engine;
using Common.Engine.Config;
using Common.Engine.Surveys;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Graph;
using Web.Server.Bots.Dialogues.Abstract;

namespace Web.Server.Bots.Dialogues;

/// <summary>
/// Entrypoint to all new conversations
/// </summary>
public class StopBotheringMeDialogue : CommonBotDialogue
{
    private readonly ILogger<StopBotheringMeDialogue> _tracer;

    public const string BTN_YES = "Yup";

    /// <summary>
    /// Setup dialogue flow
    /// </summary>
    public StopBotheringMeDialogue(BotConfig configuration, BotConversationCache botConversationCache, ILogger<StopBotheringMeDialogue> tracer,
        IServiceProvider services, GraphServiceClient graphServiceClient)
        : base(nameof(StopBotheringMeDialogue), botConversationCache, configuration, services, graphServiceClient)
    {
        _tracer = tracer;
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog),
        [
            StopBotheringMe,
            SaveDnD
        ]));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        InitialDialogId = nameof(WaterfallDialog);
    }


    /// <summary>
    /// User has said they want to stop being bothered
    /// </summary>
    private async Task<DialogTurnResult> StopBotheringMe(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
        {
            Prompt = BuildMsg("ARE YOU SURE?"),
            Choices = new List<Choice>() {
                    new Choice() { Value = BTN_YES, Synonyms = new List<string>() { "Yes", "Do it", "Send" } },
                    new Choice() { Value = "Nah", Synonyms = new List<string>() { "No", "Stop", "Abort" } }
                }
        }, cancellationToken);
    }

    /// <summary>
    /// User has picked until when to stop bothering user
    /// </summary>
    private async Task<DialogTurnResult> SaveDnD(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var response = (FoundChoice)stepContext.Result;
        if (response.Value == BTN_YES)
        {
            var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);
            var chatUser = await GetCachedUser(botUser);
            if (chatUser != null && chatUser.UserPrincipalName != null)
            {
                SurveyPendingActivities? userPendingEvents = null;
                await GetSurveyManagerService(async surveyManager =>
                {
                    userPendingEvents = await GetSurveyPendingActivities(surveyManager, chatUser.UserPrincipalName);
                });

                // Register survey request sent so we don't repeatedly ask for the same event
                await GetSurveyManagerService(async surveyManager => await surveyManager.Loader.StopBotheringUser(chatUser.UserPrincipalName, DateTime.MaxValue));

                await SendMsg(stepContext.Context, "Bye then 😞. You can always say hi, and I'll always respond if I can ♥️");
            }
            else
            {
                await SendMsg(stepContext.Context, "Ooops, looks like I can't find your UPN to save your request. Are you a guest user?");
            }
        }
        else
        {
            await SendMsg(stepContext.Context, "Ok. I'll be in touch later to see how you're getting on with copilot. You can always ping me here if you want to send feedback!");
        }
        return await stepContext.CancelAllDialogsAsync(true);
    }
}
