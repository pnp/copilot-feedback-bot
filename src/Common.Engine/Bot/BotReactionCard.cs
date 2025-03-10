﻿using Common.Engine.Surveys;
using Newtonsoft.Json.Linq;

namespace Common.Engine.Bot;

public class BotReactionCard : BaseAdaptiveCard
{
    private readonly bool _isHappy;
    private readonly SurveyPage? _firstSurveyPage;

    public BotReactionCard(string reactionMessage, bool isHappy, SurveyPage? firstSurveyPage)
    {
        ReactionMessage = reactionMessage;
        _isHappy = isHappy;
        _firstSurveyPage = firstSurveyPage;
    }

    public string ReactionMessage { get; set; }


    public override string GetCardContent()
    {
        var jsonReactionSpecific = _isHappy ? ReadResource(BotConstants.BotReactionHappy) : ReadResource(BotConstants.BotReactionMeh);

        jsonReactionSpecific = jsonReactionSpecific.Replace(BotConstants.FIELD_NAME_MESSAGE, ReactionMessage);

        // Merge the common part of the card with the specific part
        var cardBody = JObject.Parse(jsonReactionSpecific);
        if (_firstSurveyPage != null)
        {
            cardBody.Merge(_firstSurveyPage.BuildAdaptiveCard(), new JsonMergeSettings
            {
                // Avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }

        return cardBody.ToString();
    }
}
