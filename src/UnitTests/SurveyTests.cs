using Common.Engine.Surveys;
using Entities.DB.Entities;

namespace UnitTests;

[TestClass]
public class SurveyTests : AbstractTest
{
    [TestMethod]
    public void SurveyModelTests()
    {
        var pageDb = new SurveyPageDB
        {
            AdaptiveCardTemplateJson = @"
                {
                  ""type"": ""AdaptiveCard"",
                  ""version"": ""1.3"",
                  ""body"": [
                    {
                      ""type"": ""TextBlock"",
                      ""size"": ""Medium"",
                      ""text"": ""Answer this thus:"",
                      ""weight"": ""Bolder""
                    }
                  ],
                  ""actions"": [
                    {
                      ""type"": ""Action.Submit"",
                      ""title"": ""Update Survey""
                    }
                  ],
                  ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json""
                }",
            Questions = new List<SurveyQuestionDB>()
            {
                new() {
                    ID = 1,
                    DataType = QuestionDatatype.String,
                    ExpectedValue = "Don't hurt me",
                    ExpectedValueLogicalOp = LogicalOperator.Equals,
                    Question = "What is love?",
                    Index = 0,
                },
                new() {
                    ID = 2,
                    DataType = QuestionDatatype.Int,
                    ExpectedValue = "2",
                    ExpectedValueLogicalOp = LogicalOperator.Equals,
                    Question = "What is 1+1?",
                    Index = 2,
                },
                new() {
                    ID = 3,
                    DataType = QuestionDatatype.Bool,
                    ExpectedValue = "True",
                    ExpectedValueLogicalOp = LogicalOperator.Equals,
                    Question = "True?",
                    Index = 4,
                },
            }
        };

        var survey = new SurveyPage(pageDb);

        // Check index values
        for (int i = 0; i < survey.AllQuestions.Count; i++)
        {
            Assert.AreEqual(survey.AllQuestions[i].Index, i);
        }

        Assert.AreEqual(3, survey.AllQuestions.Count);
        Assert.AreEqual(1, survey.StringQuestions.Count);
        Assert.AreEqual(1, survey.IntQuestions.Count);
        Assert.AreEqual(1, survey.BoolQuestions.Count);

        var json = survey.BuildAdaptiveCard();
        Assert.AreEqual("AdaptiveCard", json["type"]);
        Assert.AreEqual("1.3", json["version"]);
    }

    [TestMethod]
    public void AnswerIsPositiveResultTests()
    {

        // Check unknown logical operator. Insufficient data to determine if the answer is positive
        Assert.ThrowsException<SurveyEngineDataException>(() =>
        {
            var unknownSurveyQ = new StringSurveyAnswer
            {
                Question = new StringSurveyQuestion
                {
                    Question = "What is love?",
                    ExpectedValue = "Don't hurt me",
                },
                ValueGiven = "Don't hurt me"
            }.IsPositiveResult;
        });

        var stringEqualsSurveyQ = new StringSurveyAnswer
        {
            Question = new StringSurveyQuestion
            {
                Question = "What is love?",
                ExpectedValue = "Don't hurt me",
                ExpectedValueLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = "Don't hurt me"
        };

        // Check that the expected value is equal to the value given
        Assert.IsTrue(stringEqualsSurveyQ.IsPositiveResult);

        // Check a false value
        stringEqualsSurveyQ.Question.ExpectedValueLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(stringEqualsSurveyQ.IsPositiveResult);

        var intEqualsSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is 1+1?",
                ExpectedValue = 2,
                ExpectedValueLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = 2
        };
        Assert.IsTrue(intEqualsSurveyQ.IsPositiveResult);

        // Check a false value
        intEqualsSurveyQ.Question.ExpectedValueLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(intEqualsSurveyQ.IsPositiveResult);

        var intGTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is greater than 1+1?",
                ExpectedValue = 2,
                ExpectedValueLogicalOp = LogicalOperator.GreaterThan,
            },
            ValueGiven = 4
        };
        Assert.IsTrue(intGTSurveyQ.IsPositiveResult);

        // Check a false value
        intGTSurveyQ.Question.ExpectedValueLogicalOp = LogicalOperator.LessThan;
        Assert.IsFalse(intGTSurveyQ.IsPositiveResult);

        var intLTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is less than 10+1?",
                ExpectedValue = 10,
                ExpectedValueLogicalOp = LogicalOperator.LessThan,
            },
            ValueGiven = 9
        };

        Assert.IsTrue(intLTSurveyQ.IsPositiveResult);

        // Check a false value
        intLTSurveyQ.Question.ExpectedValueLogicalOp = LogicalOperator.GreaterThan;
        Assert.IsFalse(intLTSurveyQ.IsPositiveResult);

        var boolSurveyQ = new BooleanSurveyAnswer
        {
            Question = new BooleanSurveyQuestion
            {
                Question = "Is this true?",
                ExpectedValue = true,
                ExpectedValueLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = true
        };
        Assert.IsTrue(boolSurveyQ.IsPositiveResult);

        // Check a false value
        boolSurveyQ.Question.ExpectedValueLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(boolSurveyQ.IsPositiveResult);
    }
}
