﻿using Entities.DB.Entities;
using System.Text.Json.Serialization;

namespace Common.Engine.Surveys.Model;

public class BaseDTO
{
    public BaseDTO()
    {
    }
    public BaseDTO(AbstractEFEntity entity)
    {
        Id = entity.ID.ToString();
    }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public abstract class BaseDTOWithName : BaseDTO
{
    public BaseDTOWithName()
    {
    }
    public BaseDTOWithName(AbstractEFEntityWithName entity) : base(entity)
    {
        Name = entity.Name;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class SurveyPageDTO : BaseDTOWithName
{
    public SurveyPageDTO()
    {
    }
    public SurveyPageDTO(SurveyPageDB page) : base(page)
    {
        Questions = page.Questions.Select(q => new SurveyQuestionDTO(q)).ToList();
        PageIndex = page.PageIndex;
        AdaptiveCardTemplateJson = page.AdaptiveCardTemplateJson;
        IsPublished = page.IsPublished;
    }

    [JsonPropertyName("questions")]
    public List<SurveyQuestionDTO> Questions { get; set; } = new();

    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    [JsonPropertyName("adaptiveCardTemplateJson")]
    public string AdaptiveCardTemplateJson { get; set; } = null!;

    [JsonPropertyName("isPublished")]
    public bool IsPublished { get; set; }
}
