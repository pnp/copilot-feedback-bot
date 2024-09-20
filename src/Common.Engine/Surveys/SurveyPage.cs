using Entities.DB.Entities;
using Newtonsoft.Json.Linq;

namespace Common.Engine.Surveys;

public class SurveyPage
{
    public SurveyPage()
    {
    }

    public SurveyPage(SurveyPageDB surveyPageDB) : this()
    {
        this.SurveyPageAdaptiveCardTemplateJson = surveyPageDB.AdaptiveCardTemplateJson;

        var i = 0;
        foreach (var q in surveyPageDB.Questions.OrderBy(q => q.Index))
        {
            if (q.OptimalAnswerLogicalOp == LogicalOperator.Unknown)
            {
                throw new SurveyEngineDataException($"Unknown logical operator for question ID '{q.ID}'");
            }
            if (q.DataType == QuestionDatatype.Int)
            {
                var intQ = new IntSurveyQuestion(q, i)
                {
                    OptimalAnswer = q.OptimalAnswerValue != null ? int.Parse(q.OptimalAnswerValue) : 0
                };
                IntQuestions.Add(intQ);
                AllQuestions.Add(intQ);
            }
            else if (q.DataType == QuestionDatatype.String)
            {
                var stringQ = new StringSurveyQuestion(q, i)
                {
                    OptimalAnswer = q.OptimalAnswerValue
                };
                StringQuestions.Add(stringQ);
                AllQuestions.Add(stringQ);
            }
            else if (q.DataType == QuestionDatatype.Bool)
            {
                var boolQ = new BooleanSurveyQuestion(q, i)
                {
                    OptimalAnswer = q.OptimalAnswerValue != null ? bool.Parse(q.OptimalAnswerValue) : false
                };
                BoolQuestions.Add(boolQ);
                AllQuestions.Add(boolQ);
            }
            else
            {
                throw new SurveyEngineDataException($"Unknown question data type for question ID '{q.ID}'");
            }
            i++;
        }
    }

    public List<StringSurveyQuestion> StringQuestions { get; set; } = new();
    public List<IntSurveyQuestion> IntQuestions { get; set; } = new();
    public List<BooleanSurveyQuestion> BoolQuestions { get; set; } = new();

    public List<BaseSurveyQuestion> AllQuestions { get; set; } = new();

    public string SurveyPageAdaptiveCardTemplateJson { get; set; } = null!;
    public string SurveyPageCommonAdaptiveCardTemplateJson { get; set; } = null!;

    public JObject BuildAdaptiveCard()
    {
        if (string.IsNullOrEmpty(SurveyPageAdaptiveCardTemplateJson))
        {
            throw new SurveyEngineDataException("Adaptive card template JSON is empty");
        }
        var surveyPageCardBody = JObject.Parse(SurveyPageAdaptiveCardTemplateJson);
        var surveyPageCardBodyCommon = string.IsNullOrEmpty(SurveyPageCommonAdaptiveCardTemplateJson) ? new JObject() : JObject.Parse(SurveyPageCommonAdaptiveCardTemplateJson);

        var unionMergeSettings = new JsonMergeSettings
        {
            // Avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        };

        // Build questions adaptive card merge content
        var questionSet = new JArray();
        foreach (var q in AllQuestions)
        {
            questionSet.Add(q.GetAdaptiveCardJson());
        }
        var questionsBody = new JObject
        {
            ["body"] = questionSet
        };

        surveyPageCardBody.Merge(questionsBody, unionMergeSettings);
        surveyPageCardBody.Merge(surveyPageCardBodyCommon, unionMergeSettings);

        return surveyPageCardBody;
    }
}
