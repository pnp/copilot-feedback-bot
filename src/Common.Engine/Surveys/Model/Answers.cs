using Entities.DB.Entities;

namespace Common.Engine.Surveys.Model;

public abstract class SurveyAnswer<T> where T : notnull
{
    public SurveyAnswer()
    {
    }
    protected SurveyAnswer(SurveyAnswerDB a) : this()
    {
        this.Id = a.ID;
    }

    public int Id { get; set; } = 0;
    public T ValueGiven { get; set; } = default!;

    public SurveyQuestion<T> Question { get; set; } = default!;

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
    public StringSurveyAnswer() : base()
    {
    }
    public StringSurveyAnswer(SurveyAnswerDB a) : base(a)
    {
        Question = new StringSurveyQuestion(a.ForQuestion, 0) { OptimalAnswer = a.ForQuestion.OptimalAnswerValue };
        ValueGiven = a.GivenAnswer;
    }

    protected override bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op)
    {
        throw new InvalidOperationException("String survey questions can't be greater/less than compared");
    }
}

public class IntSurveyAnswer : SurveyAnswer<int>
{
    public IntSurveyAnswer() : base()
    {
    }
    public IntSurveyAnswer(SurveyAnswerDB a) : base(a)
    {
        Question = new IntSurveyQuestion(a.ForQuestion, 0);

        if (int.TryParse(a.GivenAnswer, out var r))
        {
            ValueGiven = r;
        }
        else
        {
            throw new SurveyEngineDataException("Failed to parse int answer");
        }
    }

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
    public BooleanSurveyAnswer() : base()
    {
    }

    public BooleanSurveyAnswer(SurveyAnswerDB a) : base(a)
    {
        Question = new BooleanSurveyQuestion(a.ForQuestion, 0);

        if (bool.TryParse(a.GivenAnswer, out var r))
        {
            ValueGiven = r;
        }
        else
        {
            throw new SurveyEngineDataException("Failed to parse boolean answer");
        }
    }

    protected override bool IsAnswerTrueForExpectedComparativeVal(LogicalOperator op)
    {
        throw new InvalidOperationException("Boolean survey questions can't be greater/less than compared");
    }
}

public class AnswersCollection
{
    public AnswersCollection()
    {
    }

    public AnswersCollection(List<SurveyAnswerDB> answers)
    {
        foreach (var a in answers)
        {
            if (a.ForQuestion.DataType == QuestionDatatype.String)
            {
                StringSurveyAnswers.Add(new StringSurveyAnswer(a));
            }
            else if (a.ForQuestion.DataType == QuestionDatatype.Int)
            {
                IntSurveyAnswers.Add(new IntSurveyAnswer(a));
            }
            else if (a.ForQuestion.DataType == QuestionDatatype.Bool)
            {
                BooleanSurveyAnswers.Add(new BooleanSurveyAnswer(a));
            }
            else
            {
                throw new SurveyEngineDataException("Unknown question datatype");
            }
        }
    }

    public List<int> AllAnswerIds =>
        StringSurveyAnswers.Select(a => a.Id)
        .Concat(IntSurveyAnswers.Select(a => a.Id))
        .Concat(BooleanSurveyAnswers.Select(a => a.Id))
        .Distinct()
        .ToList();


    public List<IntSurveyAnswer> IntSurveyAnswers { get; set; } = new();
    public List<StringSurveyAnswer> StringSurveyAnswers { get; set; } = new();
    public List<BooleanSurveyAnswer> BooleanSurveyAnswers { get; set; } = new();
}