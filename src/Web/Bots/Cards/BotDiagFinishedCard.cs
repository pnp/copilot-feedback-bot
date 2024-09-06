namespace Web.Bots.Cards;

public class BotDiagFinishedCard : BaseAdaptiveCard
{
    public BotDiagFinishedCard()
    {
    }

    protected override string GetCardContent()
    {
        var json = ReadResource(BotConstants.BotDiagFinished);

        return json;
    }
}
