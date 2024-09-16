using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityImporter.Engine.Surveys;


public class StringSurveyAnswer : SurveyAnswer<string>
{
}
public class IntSurveyAnswer : SurveyAnswer<int>
{
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

            throw new InvalidOperationException("Invalid survey data");
        }
    }
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
    public Op ExpectedValueLogicalOp { get; }
}
