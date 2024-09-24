using Entities.DB.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Engine.Surveys.Model;



public class StringSurveyQuestion : SurveyQuestion<string>
{
    public StringSurveyQuestion() : base()
    {
    }

    public StringSurveyQuestion(SurveyQuestionDB q, int index) : base(q, index)
    {
    }

    internal override JObject GetAdaptiveCardJson()
    {
        var c = base.GetAdaptiveCardJson();
        c["type"] = "Input.Text";
        return c;
    }
}
public class IntSurveyQuestion : SurveyQuestion<int>
{
    public IntSurveyQuestion() : base()
    {
    }
    public IntSurveyQuestion(SurveyQuestionDB q, int index) : base(q, index)
    {
    }
    internal override JObject GetAdaptiveCardJson()
    {
        var c = base.GetAdaptiveCardJson();
        c["type"] = "Input.Number";
        return c;
    }
}
public class BooleanSurveyQuestion : SurveyQuestion<bool>
{
    public BooleanSurveyQuestion() : base()
    {
    }
    public BooleanSurveyQuestion(SurveyQuestionDB q, int index) : base(q, index)
    {
        if (bool.TryParse(q.OptimalAnswerValue, out var r))
        {
            OptimalAnswer = r;
        }
        else
        {
            throw new SurveyEngineDataException("Failed to parse boolean OptimalAnswerValue");
        }
    }

    internal override JObject GetAdaptiveCardJson()
    {
        var c = base.GetAdaptiveCardJson();
        c["type"] = "Input.Toggle";
        c["title"] = "Yes/no";
        return c;
    }
}


public abstract class BaseSurveyQuestion
{
    public BaseSurveyQuestion()
    {
        Question = string.Empty;
    }
    protected BaseSurveyQuestion(SurveyQuestionDB q, int index)
    {
        Question = q.Question;
        OptimalAnswerLogicalOp = q.OptimalAnswerLogicalOp ?? LogicalOperator.Unknown;
        Id = q.ID;
        Index = index;
    }

    public string Question { get; set; }
    public int Index { get; set; }
    public int Id { get; set; } = 0;
    public LogicalOperator OptimalAnswerLogicalOp { get; set; } = LogicalOperator.Unknown;

    internal virtual JObject GetAdaptiveCardJson()
    {
        if (Id == 0)
        {
            throw new SurveyEngineDataException("Question ID is not set");
        }

        // Build common properties
        return new JObject
        {
            ["id"] = $"autoQuestionId-{Id}",
            ["label"] = this.Question
        };
    }
}
public abstract class SurveyQuestion<T> : BaseSurveyQuestion
{
    public SurveyQuestion() : base()
    {
    }
    protected SurveyQuestion(SurveyQuestionDB q, int index) : base(q, index)
    {
    }

    public T? OptimalAnswer { get; set; } = default!;
}


public class SurveyPageUserResponse
{
    public SurveyPageUserResponse(string? jsonFromAdaptiveCard, string? userPrincipalName)
    {
        if (string.IsNullOrEmpty(userPrincipalName) || string.IsNullOrEmpty(jsonFromAdaptiveCard))
        {
            IsValid = false;
            return;
        }
        try
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFromAdaptiveCard);
            IsValid = true;

            if (values == null)
            {
                throw new SurveyEngineDataException("Failed to deserialize survey response");
            }
            foreach (var kvp in values)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                {
                    var id = kvp.Key.Replace("autoQuestionId-", "");
                    if (int.TryParse(id, out var questionId))
                    {
                        var a = new RawResponse { QuestionId = questionId, Response = kvp.Value };
                        Answers.Add(a);
                    }
                }
            }
        }
        catch (JsonReaderException)
        {
            IsValid = false;
            return;
        }

        IsValid = !string.IsNullOrEmpty(userPrincipalName);
        UserPrincipalName = userPrincipalName;
    }

    public bool IsValid { get; }

    public string UserPrincipalName { get; set; } = string.Empty;
    public List<RawResponse> Answers { get; set; } = new();
    public List<int> QuestionIds => Answers.Select(a => a.QuestionId).ToList();

    public record RawResponse
    {
        public required int QuestionId { get; set; }
        public required string Response { get; set; }
    }
}
