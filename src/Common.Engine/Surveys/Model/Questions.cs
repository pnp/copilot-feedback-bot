﻿using Entities.DB.Entities;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Common.Engine.Surveys.Model;

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
        if (bool.TryParse(q.OptimalAnswerValue, out var r))
        {
            OptimalAnswer = r;
        }
        else
        {
            throw new SurveyEngineDataException("Failed to parse boolean OptimalAnswerValue");
        }
    }

    internal override JObject GetAdaptiveCardJson()
    {
        var c = base.GetAdaptiveCardJson();
        c["type"] = "Input.Toggle";
        c["title"] = "Yes/no";
        return c;
    }
}


public abstract class BaseSurveyQuestion : BaseDTO
{
    public BaseSurveyQuestion()
    {
        Question = string.Empty;
    }
    protected BaseSurveyQuestion(SurveyQuestionDB q, int index) : base(q)
    {
        Question = q.Question;
        OptimalAnswerLogicalOp = q.OptimalAnswerLogicalOp ?? LogicalOperator.Unknown;
        Index = index;
    }

    public string Question { get; set; }
    public int Index { get; set; }
    public LogicalOperator OptimalAnswerLogicalOp { get; set; } = LogicalOperator.Unknown;

    internal virtual JObject GetAdaptiveCardJson()
    {
        if (string.IsNullOrEmpty(Id))
        {
            throw new SurveyEngineDataException("Question ID is not set");
        }

        // Build common properties
        return new JObject
        {
            ["id"] = $"autoQuestionId-{Id}",
            ["label"] = this.Question
        };
    }
}

public class SurveyQuestionDTO : BaseDTO
{
    public SurveyQuestionDTO()
    {
    }
    public SurveyQuestionDTO(SurveyQuestionDB q) : base(q)
    {
        this.Question = q.Question;
        this.Index = q.Index;
        this.OptimalAnswerLogicalOp = q.OptimalAnswerLogicalOp;
        this.OptimalAnswerValue = q.OptimalAnswerValue;
        this.DataType = q.DataType;
        this.ForSurveyPageId = q.ForSurveyPage.ID.ToString();
    }

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int Index { get; set; } = 0;

    [JsonPropertyName("optimalAnswerLogicalOp")]
    public LogicalOperator? OptimalAnswerLogicalOp { get; set; }

    [JsonPropertyName("optimalAnswerValue")]
    public string? OptimalAnswerValue { get; set; }

    [JsonPropertyName("dataType")]
    public QuestionDatatype DataType { get; set; }

    [JsonPropertyName("forSurveyPageId")]
    public string ForSurveyPageId { get; set; } = null!;
}

public abstract class SurveyQuestion<T> : BaseSurveyQuestion
{
    public SurveyQuestion() : base()
    {
    }
    protected SurveyQuestion(SurveyQuestionDB q, int index) : base(q, index)
    {
    }

    public T? OptimalAnswer { get; set; } = default!;
}

