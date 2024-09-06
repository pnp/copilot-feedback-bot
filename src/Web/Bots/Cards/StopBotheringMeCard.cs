namespace Web.Bots.Cards;

public class StopBotheringMeCard : BaseAdaptiveCard
{

    protected override string GetCardContent()
    {
        var json = ReadResource(BotConstants.StopBotheringMe);
        return json;
    }
}
