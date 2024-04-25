using Newtonsoft.Json.Linq;

namespace Web.Bots.Cards;

public class BotReactionCard : BaseAdaptiveCard
{
    private readonly bool _isHappy;

    public BotReactionCard(string message, bool isHappy)
    {
        Message = message;
        _isHappy = isHappy;
    }

    public string Message { get; set; }


    public override string GetCardContent()
    {
        var jsonReactionSpecific = _isHappy ? ReadResource(BotConstants.BotReactionHappy) : ReadResource(BotConstants.BotReactionMeh);

        jsonReactionSpecific = jsonReactionSpecific.Replace(BotConstants.FIELD_NAME_MESSAGE, Message);

        // Merge the common part of the card with the specific part
        var cardBody = JObject.Parse(jsonReactionSpecific);
        cardBody.Merge(JObject.Parse(ReadResource(BotConstants.BotReactionCommon)), new JsonMergeSettings
        {
            // Avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return cardBody.ToString();
    }
}
