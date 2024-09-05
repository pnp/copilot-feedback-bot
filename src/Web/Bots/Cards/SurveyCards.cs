using Entities.DB.Entities.AuditLog;
using Newtonsoft.Json.Linq;

namespace Web.Bots.Cards;

public abstract class CommonCardWithStartOrStopSurveysButtonCard: BaseAdaptiveCard
{
    private readonly bool _userHasSurveysStopped;

    protected CommonCardWithStartOrStopSurveysButtonCard(bool userHasSurveysStopped)
    {
        _userHasSurveysStopped = userHasSurveysStopped;
    }

    protected string GetMergedCardWithSurveyButtonContent(string cardSpecific)
    {
        var cardBody = JObject.Parse(cardSpecific);

        var buttonBody = _userHasSurveysStopped ? JObject.Parse(ReadResource(BotConstants.SurveyButtonContinueSurveys)) :
            JObject.Parse(ReadResource(BotConstants.SurveyButtonStopBotheringMe));

        cardBody.Merge(buttonBody, new JsonMergeSettings
        {
            // Avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return cardBody.ToString();
    }
}

public abstract class SurveyCard : CommonCardWithStartOrStopSurveysButtonCard
{
    public SurveyCard(bool userHasSurveysStopped) : base(userHasSurveysStopped)
    {
        // Replace in JSON the variables with the actual values
        CardBody = ReadResource(BotConstants.SurveyCommonBody)
            .Replace(GetJsonVarName(nameof(BotConstants.SurveyAnswerRating1)), BotConstants.SurveyAnswerRating1)
            .Replace(GetJsonVarName(nameof(BotConstants.SurveyAnswerRating2)), BotConstants.SurveyAnswerRating2)
            .Replace(GetJsonVarName(nameof(BotConstants.SurveyAnswerRating3)), BotConstants.SurveyAnswerRating3)
            .Replace(GetJsonVarName(nameof(BotConstants.SurveyAnswerRating4)), BotConstants.SurveyAnswerRating4)
            .Replace(GetJsonVarName(nameof(BotConstants.SurveyAnswerRating5)), BotConstants.SurveyAnswerRating5)
            .Replace(BotConstants.FIELD_NAME_SURVEY_STOP, BotConstants.SurveyAnswerStop);
    }
    string GetJsonVarName(string name) => "${" + name + "}";

    public string CardBody { get; set; }

    protected string GetMergedCardContent(string cardSpecific)
    {
        var cardBody = JObject.Parse(cardSpecific);
        cardBody.Merge(JObject.Parse(CardBody), new JsonMergeSettings
        {
            // Avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return base.GetMergedCardWithSurveyButtonContent(cardBody.ToString());
    }
}


public class CopilotTeamsActionSurveyCard : SurveyCard
{

    private readonly CopilotEventMetadataMeeting _baseCopilotEvent;

    public CopilotTeamsActionSurveyCard(CopilotEventMetadataMeeting baseCopilotEvent, bool userHasSurveysStopped) : base(userHasSurveysStopped)
    {
        _baseCopilotEvent = baseCopilotEvent;
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.CardFileNameCopilotTeamsActionSurvey);
        json = json.Replace(BotConstants.FIELD_NAME_RESOURCE_NAME, _baseCopilotEvent.GetEventDescription());
        return GetMergedCardContent(json);
    }
}

public class CopilotFileActionSurveyCard : SurveyCard
{
    private readonly CopilotEventMetadataFile _baseCopilotEvent;

    public CopilotFileActionSurveyCard(CopilotEventMetadataFile baseCopilotEvent, bool userHasSurveysStopped) : base(userHasSurveysStopped)
    {
        _baseCopilotEvent = baseCopilotEvent;
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.CardFileNameCopilotFileActionSurvey);
        json = json.Replace(BotConstants.FIELD_NAME_RESOURCE_NAME, _baseCopilotEvent.GetEventDescription());
        return GetMergedCardContent(json);
    }
}

public class SurveyNotForSpecificAction : SurveyCard
{
    public SurveyNotForSpecificAction(bool userHasSurveysStopped) : base(userHasSurveysStopped)
    {
    }

    public override string GetCardContent()
    {
        var json = ReadResource(BotConstants.CardFileNameSurveyNoActionCard);
        return GetMergedCardContent(json);
    }
}
