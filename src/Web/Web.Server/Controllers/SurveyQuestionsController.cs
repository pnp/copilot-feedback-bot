using Common.Engine.Surveys;
using Entities.DB;
using Entities.DB.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SurveyQuestionsController(ILogger<SurveyQuestionsController> logger, DataContext context) : ControllerBase
{
    // GET: api/SurveyQuestions
    [HttpGet]
    public async Task<List<SurveyPageEditViewModel>> SurveyQuestions()
    {
        logger.LogInformation("Called GetSurveyQuestions");
        var allPages = await context.SurveyPages
            .Include(d=> d.Questions)
            .ToListAsync();
        return allPages.Select(p=> new SurveyPageEditViewModel(p)).ToList();
    }

}

/// <summary>
/// Same as DB record but with merged adaptive card with questions
/// </summary>
public class SurveyPageEditViewModel : SurveyPageDB
{
    public SurveyPageEditViewModel()
    {
    }

    public SurveyPageEditViewModel(SurveyPageDB surveyPageDB)
    {
        this.ID = surveyPageDB.ID;
        this.AdaptiveCardTemplateJson = surveyPageDB.AdaptiveCardTemplateJson;
        this.Questions = surveyPageDB.Questions;
        this.IsPublished = surveyPageDB.IsPublished;
        this.PageIndex = surveyPageDB.PageIndex;

        var model = new SurveyPage(surveyPageDB);
        this.AdaptiveCardTemplateJsonWithQuestions = model.BuildAdaptiveCard().ToString();
    }

    public string AdaptiveCardTemplateJsonWithQuestions { get; set; } = null!;
}
