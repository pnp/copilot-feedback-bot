namespace Web.Bots.Cards;

public class StopBotheringMeCard : BaseAdaptiveCard
{

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.StopBotheringMe);
        return json;
    }
}
