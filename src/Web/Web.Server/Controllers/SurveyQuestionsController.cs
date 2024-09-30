using Azure.Data.Tables;
using Common.Engine.Config;
using Entities.DB;
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
    [HttpPost(nameof(GetSurveyQuestions))]
    public async Task< IActionResult> GetSurveyQuestions()
    {
        var allPages = await context.SurveyPages.ToListAsync();
        return Ok(allPages);
    }

}
