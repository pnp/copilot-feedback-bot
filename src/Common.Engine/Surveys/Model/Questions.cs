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

