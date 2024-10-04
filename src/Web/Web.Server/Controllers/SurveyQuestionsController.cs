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
    public async Task<List<SurveyPageDB>> SurveyQuestions()
    {
        logger.LogInformation("Called GetSurveyQuestions");
        var allPages = await context.SurveyPages
            .Include(d=> d.Questions)
            .ToListAsync();
        return allPages;
    }

    // POST: api/SurveyQuestions
    [HttpPost]
    public async Task<List<SurveyPageDB>> SavePage([FromBody]SurveyPageDB page)
    {
        logger.LogInformation("Called SavePage");
        context.SurveyPages.Update(page);
        await context.SaveChangesAsync();

        return await SurveyQuestions();
    }
}
