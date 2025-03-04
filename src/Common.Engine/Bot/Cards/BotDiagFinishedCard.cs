using Common.Engine.Surveys;

namespace Common.Engine.Bot.Cards;

public class BotDiagFinishedCard : BaseAdaptiveCard
{
    public BotDiagFinishedCard()
    {
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.BotDiagFinished);

        return json;
    }
}
