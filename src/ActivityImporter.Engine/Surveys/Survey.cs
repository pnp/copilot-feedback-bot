namespace ActivityImporter.Engine.Surveys;


public class StringSurveyAnswer : SurveyAnswer<string>
{
    protected override bool IsAnswerTrueForExpectedComparativeVal(Op op)
    {
        throw new InvalidOperationException("String survey questions can't be greater/less than compared");
    }
}
public class IntSurveyAnswer : SurveyAnswer<int>
{
    protected override bool IsAnswerTrueForExpectedComparativeVal(Op op)
    {
        switch (op)
        {
            case Op.Equals:
                return ValueGiven == Question.ExpectedValue;
            case Op.NotEquals:
                return ValueGiven != Question.ExpectedValue;
            case Op.GreaterThan:
                return ValueGiven > Question.ExpectedValue;
            case Op.LessThan:
                return ValueGiven < Question.ExpectedValue;
            default:
                throw new SurveyEngineLogicException();
        }
    }
}

public abstract class SurveyAnswer<T>
{
    public required T ValueGiven { get; set; }

    public required SurveyQuestion<T> Question { get; set; }

    public bool IsPositiveResult
    {
        get
        {
            if (ValueGiven == null) return false;

            if (Question.ExpectedValueLogicalOp == Op.Equals)
            {
                return ValueGiven.Equals(Question.ExpectedValue);
            }
            else if (Question.ExpectedValueLogicalOp == Op.NotEquals)
            {
                return !ValueGiven.Equals(Question.ExpectedValue);
            }
            else if (Question.ExpectedValueLogicalOp == Op.GreaterThan)
            {
                return IsAnswerTrueForExpectedComparativeVal(Op.GreaterThan);
            }
            else if (Question.ExpectedValueLogicalOp == Op.LessThan)
            {
                return IsAnswerTrueForExpectedComparativeVal(Op.LessThan);
            }

            throw new SurveyEngineLogicException();
        }
    }

    protected abstract bool IsAnswerTrueForExpectedComparativeVal(Op op);
}

public class SurveyEngineLogicException : Exception
{
}

public class StringSurveyQuestion : SurveyQuestion<string>
{
}
public class IntSurveyQuestion : SurveyQuestion<int>
{
}

public enum Op
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
}

public abstract class SurveyQuestion<T>
{
    public required string Question { get; set; }
    public required T ExpectedValue { get; set; }
    public Op ExpectedValueLogicalOp { get; set; }
}
