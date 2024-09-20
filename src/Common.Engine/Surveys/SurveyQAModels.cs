using Entities.DB.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Engine.Surveys;

#region Answers

public abstract class SurveyAnswer<T> where T : notnull
{
    public required T ValueGiven { get; set; }

    public required SurveyQuestion<T> Question { get; set; }

    public bool IsPositiveResult
    {
        get
        {
            if (Question.OptimalAnswerLogicalOp == LogicalOperator.Unknown)
                throw new SurveyEngineDataException($"{nameof(Question.OptimalAnswerLogicalOp)} is {nameof(LogicalOperator.Unknown)}");

            if (Question.OptimalAnswerLogicalOp == LogicalOperator.Equals)
            {
                return ValueGiven.Equals(Question.OptimalAnswer);
            }
            else if (Question.OptimalAnswerLogicalOp == LogicalOperator.NotEquals)
            {
                return !ValueGiven.Equals(Question.OptimalAnswer);
            }
            else if (Question.OptimalAnswerLogicalOp == LogicalOperator.GreaterThan)
            {
                return IsAnswerTrueForExpectedComparativeVal(LogicalOperator.GreaterThan);
            }
            else if (Question.OptimalAnswerLogicalOp == LogicalOperator.LessThan)
            {
                return IsAnswerTrueForExpectedComparativeVal(LogicalOperator.LessThan);
            }

            throw new SurveyEngineLogicException();
        }
    }

    protected abstract bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op);
}

public class StringSurveyAnswer : SurveyAnswer<string>
{
    protected override bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op)
    {
        throw new InvalidOperationException("String survey questions can't be greater/less than compared");
    }
}
public class IntSurveyAnswer : SurveyAnswer<int>
{
    protected override bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op)
    {
        switch (op)
        {
            case LogicalOperator.Equals:
                return ValueGiven == Question.OptimalAnswer;
            case LogicalOperator.NotEquals:
                return ValueGiven != Question.OptimalAnswer;
            case LogicalOperator.GreaterThan:
                return ValueGiven > Question.OptimalAnswer;
            case LogicalOperator.LessThan:
                return ValueGiven < Question.OptimalAnswer;
            default:
                throw new SurveyEngineLogicException();
        }
    }
}

public class BooleanSurveyAnswer : SurveyAnswer<bool>
{
    protected override bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op)
    {
        throw new InvalidOperationException("Boolean survey questions can't be greater/less than compared");
    }
}

#endregion


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
    }

    internal override JObject GetAdaptiveCardJson()
    {
        var c = base.GetAdaptiveCardJson();
        c["type"] = "Input.Toggle";
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
        this.Question = q.Question;
        this.OptimalAnswerLogicalOp = q.OptimalAnswerLogicalOp;
        this.Id = q.ID;
        Index = index;
    }

    public string Question { get; set; }
    public int Index { get; set; }
    public int Id { get; set; } = 0;
    public LogicalOperator? OptimalAnswerLogicalOp { get; set; }

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
            ["placeholder"] = Question,
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

    public required T? OptimalAnswer { get; set; }
}


public class SurveyResponse
{
    public SurveyResponse(string? json, string? userPrincipalName)
    {
        if (string.IsNullOrEmpty(userPrincipalName) || string.IsNullOrEmpty(json))
        {
            IsValid = false;
            return;
        }
        try
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            this.IsValid = true;

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
            this.IsValid = false;
            return;
        }

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
