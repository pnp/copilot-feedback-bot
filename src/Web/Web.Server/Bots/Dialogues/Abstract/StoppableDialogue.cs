using Common.Engine;
using Common.Engine.Config;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Graph;

namespace Web.Bots.Dialogues.Abstract;

/// <summary>
/// A dialogue that can be stopped by the user and told to stop bothering them
/// </summary>
public abstract class StoppableDialogue : CommonBotDialogue
{
    protected StoppableDialogue(string id, BotConversationCache botConversationCache, BotConfig botConfig, IServiceProvider services, GraphServiceClient graphServiceClient)
        : base(id, botConversationCache, botConfig, services, graphServiceClient)
    {
    }


    protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
    {
        var result = await InterruptAsync(innerDc, cancellationToken);
        if (result != null)
        {
            return result;
        }

        return await base.OnContinueDialogAsync(innerDc, cancellationToken);
    }

    protected async Task<DialogTurnResult?> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
    {
        if (innerDc.Context.Activity.Type == ActivityTypes.Message)
        {
            if (innerDc.Context.Activity.Text == BotConstants.SurveyAnswerStop)
            {
                return await innerDc.BeginDialogAsync(nameof(StopBotheringMeDialogue), null, cancellationToken);
            }
        }

        return null;
    }

}
