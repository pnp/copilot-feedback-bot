using Common.Engine.Surveys;
using Common.Engine.Surveys.Model;
using Common.Engine.UsageStats;
using Entities.DB.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Web.Server.Controllers;

/// <summary>
/// Return stats for bot surveys
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StatsController(ILogger<SurveyQuestionsController> logger, DataContext context, ReportManager reportManager, SurveyManager surveyManager) : ControllerBase
{
    // GET: api/Stats/GetBasicStats
    [HttpGet(nameof(GetBasicStats))]
    public async Task<BasicStats> GetBasicStats()
    {
        logger.LogInformation("Called GetBasicStats");

        var usersSurveyed = await context.SurveyGeneralResponses
            .Select(r => r.User.UserPrincipalName)
            .Distinct()
            .CountAsync();

        var usersResponded = await context.SurveyGeneralResponses
            .Where(r => r.Responded != null)
            .Select(r => r.User.UserPrincipalName)
            .Distinct()
            .CountAsync();

        var usersNotResponded = usersSurveyed - usersResponded;

        var usersFound = await context.Users.CountAsync();

        return new BasicStats
        {
            UsersSurveyed = usersSurveyed,
            UsersResponded = usersResponded,
            UsersNotResponded = usersNotResponded,
            UsersFound = usersFound
        };
    }

    // POST: api/Stats/GetUsageStatsReport
    [HttpPost(nameof(GetUsageStatsReport))]
    public async Task<UsageStatsReport> GetUsageStatsReport(LoaderUsageStatsReportFilter filter)
    {
        return await reportManager.GetReport(filter);
    }

    // GET: api/Stats/GetSurveyAnswers
    [HttpGet(nameof(GetSurveyAnswers))]
    public async Task<SurveyAnswersCollection> GetSurveyAnswers()
    {
        logger.LogInformation("Called GetSurveyAnswers");
        return await surveyManager.GetSurveyQuestionResponses();
    }
}

public class BasicStats
{
    [JsonPropertyName("usersSurveyed")]
    public int UsersSurveyed { get; set; }

    [JsonPropertyName("usersResponded")]
    public int UsersResponded { get; set; }

    [JsonPropertyName("usersNotResponded")]
    public int UsersNotResponded { get; set; }

    [JsonPropertyName("usersFound")]
    public int UsersFound { get; set; }
}
