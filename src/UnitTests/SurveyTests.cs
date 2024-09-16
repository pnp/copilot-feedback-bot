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
    }
}
