using Common.Engine.Surveys;
using Entities.DB.Entities;
using UnitTests.FakeLoaderClasses;

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
                    OptimalAnswerValue = "Don't hurt me",
                    OptimalAnswerLogicalOp = LogicalOperator.Equals,
                    Question = "What is love?",
                    Index = 0,
                },
                new() {
                    ID = 2,
                    DataType = QuestionDatatype.Int,
                    OptimalAnswerValue = "2",
                    OptimalAnswerLogicalOp = LogicalOperator.Equals,
                    Question = "What is 1+1?",
                    Index = 2,
                },
                new() {
                    ID = 3,
                    DataType = QuestionDatatype.Bool,
                    OptimalAnswerValue = "True",
                    OptimalAnswerLogicalOp = LogicalOperator.Equals,
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
    public void MiscModelTests()
    {
        var responseJson = @"{""autoQuestionId-1"": ""a"", ""autoQuestionId-2"": ""1"" }";
        var test = new SurveyResponse(responseJson, "whoever");
        Assert.AreEqual(2, test.Answers.Count);
        Assert.IsTrue(test.IsValid);

        Assert.IsTrue(test.Answers.Contains(new SurveyResponse.RawResponse { QuestionId = 1, Response = "a" }));
        Assert.IsTrue(test.Answers.Contains(new SurveyResponse.RawResponse { QuestionId = 2, Response = "1" }));

        Assert.IsTrue(test.QuestionIds.Count == 2);
        Assert.IsTrue(test.QuestionIds.Contains(1));
        Assert.IsTrue(test.QuestionIds.Contains(2));


        var invalid1 = new SurveyResponse("rando", "whoever");
        Assert.IsFalse(invalid1.IsValid);

        var invalid2 = new SurveyResponse("{\"random-1\": \"a\"}", "whoever");
        Assert.IsTrue(invalid2.IsValid);
        Assert.AreEqual(0, invalid2.Answers.Count);
    }

    [TestMethod]
    public async Task SurveySave()
    {

        var sm = new SurveyManager(new FakeSurveyManagerDataLoader(_config), new FakeSurveyProcessor(), GetLogger<SurveyManager>());

        var testPage = new SurveyPageDB { Name = "unit test", IsPublished = true };

        await sm.SaveCustomSurveyResponse(new SurveyResponse(@"{""autoQuestionId-1"": ""a"", ""autoQuestionId-2"": ""1"" }", "whoever"));
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
                    OptimalAnswer = "Don't hurt me",
                },
                ValueGiven = "Don't hurt me"
            }.IsPositiveResult;
        });

        var stringEqualsSurveyQ = new StringSurveyAnswer
        {
            Question = new StringSurveyQuestion
            {
                Question = "What is love?",
                OptimalAnswer = "Don't hurt me",
                OptimalAnswerLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = "Don't hurt me"
        };

        // Check that the expected value is equal to the value given
        Assert.IsTrue(stringEqualsSurveyQ.IsPositiveResult);

        // Check a false value
        stringEqualsSurveyQ.Question.OptimalAnswerLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(stringEqualsSurveyQ.IsPositiveResult);

        var intEqualsSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is 1+1?",
                OptimalAnswer = 2,
                OptimalAnswerLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = 2
        };
        Assert.IsTrue(intEqualsSurveyQ.IsPositiveResult);

        // Check a false value
        intEqualsSurveyQ.Question.OptimalAnswerLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(intEqualsSurveyQ.IsPositiveResult);

        var intGTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is greater than 1+1?",
                OptimalAnswer = 2,
                OptimalAnswerLogicalOp = LogicalOperator.GreaterThan,
            },
            ValueGiven = 4
        };
        Assert.IsTrue(intGTSurveyQ.IsPositiveResult);

        // Check a false value
        intGTSurveyQ.Question.OptimalAnswerLogicalOp = LogicalOperator.LessThan;
        Assert.IsFalse(intGTSurveyQ.IsPositiveResult);

        var intLTSurveyQ = new IntSurveyAnswer
        {
            Question = new IntSurveyQuestion
            {
                Question = "What is less than 10+1?",
                OptimalAnswer = 10,
                OptimalAnswerLogicalOp = LogicalOperator.LessThan,
            },
            ValueGiven = 9
        };

        Assert.IsTrue(intLTSurveyQ.IsPositiveResult);

        // Check a false value
        intLTSurveyQ.Question.OptimalAnswerLogicalOp = LogicalOperator.GreaterThan;
        Assert.IsFalse(intLTSurveyQ.IsPositiveResult);

        var boolSurveyQ = new BooleanSurveyAnswer
        {
            Question = new BooleanSurveyQuestion
            {
                Question = "Is this true?",
                OptimalAnswer = true,
                OptimalAnswerLogicalOp = LogicalOperator.Equals,
            },
            ValueGiven = true
        };
        Assert.IsTrue(boolSurveyQ.IsPositiveResult);

        // Check a false value
        boolSurveyQ.Question.OptimalAnswerLogicalOp = LogicalOperator.NotEquals;
        Assert.IsFalse(boolSurveyQ.IsPositiveResult);
    }
}
