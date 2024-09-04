using Common.Engine;
using Common.Engine.Config;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Graph;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web.Bots.Cards;
using Web.Bots.Dialogues.Abstract;

namespace Web.Bots.Dialogues;

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
        return await PromptWithCard(stepContext, new StopBotheringMeCard());
    }

    /// <summary>
    /// User has picked until when to stop bothering user
    /// </summary>
    private async Task<DialogTurnResult> SaveDnD(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var stopBotherJsonResponse = JsonSerializer.Deserialize<TalkToMeInResposne>(stepContext.Result?.ToString() ?? string.Empty);
        if (stopBotherJsonResponse != null)
        {
            var nextContact = DateTime.MaxValue;

            if (stopBotherJsonResponse.TalkToMeInDaysFromNow.HasValue)
            {
                nextContact = DateTime.Now.AddDays(stopBotherJsonResponse.TalkToMeInDaysFromNow.Value);
            }

            var botUser = await BotUserUtils.GetBotUserAsync(stepContext.Context, _botConfig, _graphServiceClient);
            var chatUser = await base.GetCachedUser(botUser);
            if (chatUser != null && chatUser.UserPrincipalName != null)
            {
                // Remember the user's choice
                await base.GetSurveyManagerService(async surveyManager => await surveyManager.Loader.StopBotheringUser(chatUser.UserPrincipalName, DateTime.MaxValue));

                if (stopBotherJsonResponse.TalkToMeInDaysFromNow.HasValue)
                {
                    await SendMsg(stepContext.Context, "No problem; speak soon");
                }
                else
                {
                    await SendMsg(stepContext.Context, "Bye then 😞. You can always say hi, and I'll always respond if I can ♥️");
                }
            }
            else
            {
                await SendMsg(stepContext.Context, "Ooops, looks like I can't find your UPN to save your request. Are you a guest user?");
            }
        }
        else
        {
            await SendMsg(stepContext.Context, "Ooops, I didn't understand that response. Try again?");
        }
        return await stepContext.CancelAllDialogsAsync(true);
    }

    class TalkToMeInResposne
    {
        [JsonPropertyName("talkToMeDayFromNow")]
        public int? TalkToMeInDaysFromNow { get; set; }
    }
}
