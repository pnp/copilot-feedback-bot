using ActivityImporter.Engine.Surveys;

namespace UnitTests.ActivityImporter;

[TestClass]
public class SurveyTests : AbstractTest
{
    [TestMethod]
    public void SurveyTestsAll()
    {
        var stringEqualsSurveyQ = new StringSurveyAnswer
        {
            Question = new StringSurveyQuestion
            {
                Question = "What is love?",
                ExpectedValue = "Don't hurt me",
            },
            ValueGiven = "Don't hurt me"
        };
        Assert.IsTrue(stringEqualsSurveyQ.IsPositiveResult);
        stringEqualsSurveyQ.Question.ExpectedValueLogicalOp = Op.NotEquals;

        Assert.IsFalse(stringEqualsSurveyQ.IsPositiveResult);

        var intEqualsSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is 1+1?",
                ExpectedValue = 2,
            },
            ValueGiven = 2
        };
        Assert.IsTrue(intEqualsSurveyQ.IsPositiveResult);
        var intGTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is greater than 1+1?",
                ExpectedValue = 2,
                ExpectedValueLogicalOp = Op.GreaterThan,
            },
            ValueGiven = 4
        };
        Assert.IsTrue(intGTSurveyQ.IsPositiveResult);

        var intLTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is less than 10+1?",
                ExpectedValue = 10,
                ExpectedValueLogicalOp = Op.LessThan,
            },
            ValueGiven = 9
        };

        Assert.IsTrue(intLTSurveyQ.IsPositiveResult);
    }
}
