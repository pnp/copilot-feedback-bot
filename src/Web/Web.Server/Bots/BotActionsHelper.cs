using Common.Engine.Bot;
using Common.Engine.Bot.Cards;
using Common.Engine.Config;
using Microsoft.Bot.Builder;

namespace Web.Server.Bots;

public class BotActionsHelper
{
    private readonly ILogger<BotActionsHelper> _logger;
    public BotActionsHelper(BotConfig config, ILogger<BotActionsHelper> logger)
    {
        _logger = logger;

        _logger.LogInformation($"Have config: MicrosoftAppId:{config.BotAppId}, AppCatalogTeamAppId:{config.AppCatalogTeamAppId}");
    }

    public async Task SendBotFirstIntro(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        // "Hi I'm bot..."
        var introCardAttachment = new BotFirstIntroduction(BotConstants.BotName).GetCardAttachment();
        await turnContext.SendActivityAsync(MessageFactory.Attachment(introCardAttachment), cancellationToken);
    }

    public async Task SendBotResumeConvo(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        // "Hi I'm bot..."
        var introCardAttachment = new BotResumeConversationIntroduction(BotConstants.BotName).GetCardAttachment();
        await turnContext.SendActivityAsync(MessageFactory.Attachment(introCardAttachment), cancellationToken);
    }
}
