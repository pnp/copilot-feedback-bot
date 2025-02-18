using Common.Engine.Bot;
using Common.Engine.Surveys;

namespace Web.Server.Bots.Cards;

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
