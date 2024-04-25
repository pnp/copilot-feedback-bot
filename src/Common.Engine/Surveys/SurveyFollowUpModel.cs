using System.Text.Json.Serialization;

namespace Common.Engine.Surveys;

/// <summary>
/// Responses to follow-up questions after a survey.
/// 1 = Strongly Disagree, 2 = Disagree, 3 = Agree, 4 = Strongly Agree
/// </summary>
public class SurveyFollowUpModel
{
    [JsonPropertyName("timeSaved")]
    public string TimeSavedMinutesStr { get; set; } = string.Empty;

    [JsonPropertyName("comments")]
    public string? Comments { get; set; } = null;

    public int TimeSavedMinutes => !string.IsNullOrEmpty(TimeSavedMinutesStr) ? int.Parse(TimeSavedMinutesStr) : 0;

    [JsonPropertyName("cmbCopilotImprovesQualityOfWork")]
    public string? CopilotImprovesQualityOfWorkAgreeRatingStr { get; set; }

    [JsonPropertyName("cmbCopilotHelpsWithMundaneTasks")]
    public string? CopilotHelpsWithMundaneTasksAgreeRatingStr { get; set; }

    [JsonPropertyName("cmbCopilotMakesMeMoreProductive")]
    public string? CopilotMakesMeMoreProductiveAgreeRatingStr { get; set; }

    [JsonPropertyName("cmbCopilotAllowsTaskCompletionFaster")]
    public string? CopilotAllowsTaskCompletionFasterAgreeRatingStr { get; set; }


    public QuestionRating? CopilotImprovesQualityOfWorkAgreeRating => ParseStringToRating(CopilotImprovesQualityOfWorkAgreeRatingStr);

    public QuestionRating? CopilotHelpsWithMundaneTasksAgreeRating => ParseStringToRating(CopilotHelpsWithMundaneTasksAgreeRatingStr);

    public QuestionRating? CopilotMakesMeMoreProductiveAgreeRating => ParseStringToRating(CopilotMakesMeMoreProductiveAgreeRatingStr);

    public QuestionRating? CopilotAllowsTaskCompletionFasterAgreeRating => ParseStringToRating(CopilotAllowsTaskCompletionFasterAgreeRatingStr);
    private QuestionRating? ParseStringToRating(string? strVal)
    {
        if (strVal == null)
        {
            return null;
        }
        return strVal switch
        {
            "1" => QuestionRating.StronglyDisagree,
            "2" => QuestionRating.Disagree,
            "3" => QuestionRating.Agree,
            "4" => QuestionRating.StronglyAgree,
            _ => null
        };
    }

}

public enum QuestionRating
{
    Unknown = 0,
    StronglyDisagree = 1,
    Disagree = 2,
    Agree = 3,
    StronglyAgree = 4
}
