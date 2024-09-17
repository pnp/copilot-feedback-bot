using Entities.DB.Entities;
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
            if (Question.ExpectedValueLogicalOp == LogicalOperator.Unknown)
                throw new SurveyEngineDataException($"{nameof(Question.ExpectedValueLogicalOp)} is {nameof(LogicalOperator.Unknown)}");

            if (Question.ExpectedValueLogicalOp == LogicalOperator.Equals)
            {
                return ValueGiven.Equals(Question.ExpectedValue);
            }
            else if (Question.ExpectedValueLogicalOp == LogicalOperator.NotEquals)
            {
                return !ValueGiven.Equals(Question.ExpectedValue);
            }
            else if (Question.ExpectedValueLogicalOp == LogicalOperator.GreaterThan)
            {
                return IsAnswerTrueForExpectedComparativeVal(LogicalOperator.GreaterThan);
            }
            else if (Question.ExpectedValueLogicalOp == LogicalOperator.LessThan)
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
                return ValueGiven == Question.ExpectedValue;
            case LogicalOperator.NotEquals:
                return ValueGiven != Question.ExpectedValue;
            case LogicalOperator.GreaterThan:
                return ValueGiven > Question.ExpectedValue;
            case LogicalOperator.LessThan:
                return ValueGiven < Question.ExpectedValue;
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
        this.ExpectedValueLogicalOp = q.ExpectedValueLogicalOp;
        this.Id = q.ID; 
        Index = index;
    }

    public string Question { get; set; }
    public int Index { get; set; }
    public int Id { get; set; } = 0;
    public LogicalOperator ExpectedValueLogicalOp { get; set; }

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

    public required T ExpectedValue { get; set; }
}
