using Common.Engine.Surveys;
using Newtonsoft.Json.Linq;

namespace Common.Engine.Bot.Cards;

public class CustomSurveyPageCard : BaseAdaptiveCard
{
    private readonly SurveyPage _surveyPage;

    public CustomSurveyPageCard(SurveyPage surveyPage)
    {
        _surveyPage = surveyPage;
    }

    public override string GetCardContent()
    {
        var cardBody = JObject.Parse(ReadResource(BotConstants.SurveyCustomPageCommon));

        cardBody.Merge(_surveyPage.BuildAdaptiveCard(), new JsonMergeSettings
        {
            // Avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return cardBody.ToString();
    }
}
