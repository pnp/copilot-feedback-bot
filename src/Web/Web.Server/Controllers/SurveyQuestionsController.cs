using Common.Engine.Surveys.Model;
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
    public async Task<List<SurveyPageDTO>> SurveyQuestions()
    {
        logger.LogInformation("Called GetSurveyQuestions");
        var allPages = await context.SurveyPages
            .Include(d => d.Questions)
            .ToListAsync();
        return allPages.Select(p => new SurveyPageDTO(p)).ToList();
    }

    // POST: api/SurveyQuestions
    [HttpPost]
    public async Task<List<SurveyPageDTO>> SavePage([FromBody] SurveyPageDTO pageUpdate)
    {
        logger.LogInformation("Called SavePage");

        SurveyPageDB? dbPage = null;
        if (!string.IsNullOrEmpty(pageUpdate.Id))
        {
            dbPage = await context.SurveyPages
                .Include(d => d.Questions)
                .FirstOrDefaultAsync(d => d.ID == int.Parse(pageUpdate.Id!));
        }
        if (dbPage == null)
        {
            dbPage = new SurveyPageDB();
            context.SurveyPages.Add(dbPage);
        }

        dbPage.Name = pageUpdate.Name;
        dbPage.PageIndex = pageUpdate.PageIndex;
        dbPage.AdaptiveCardTemplateJson = pageUpdate.AdaptiveCardTemplateJson;
        dbPage.IsPublished = pageUpdate.IsPublished;
        dbPage.Questions.Clear();
        foreach (var q in pageUpdate.Questions)
        {
            var dbQ = new SurveyQuestionDB
            {
                Question = q.Question,
                DataType = q.DataType,
                Index = q.Index,
                OptimalAnswerValue = q.OptimalAnswerValue,
                OptimalAnswerLogicalOp = q.OptimalAnswerLogicalOp,
                ForSurveyPage = dbPage,
            };
            dbPage.Questions.Add(dbQ);
        }

        await context.SaveChangesAsync();

        return await SurveyQuestions();
    }
}
