using Entities.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Web.Controllers;

/// <summary>
/// Return stats for bot surveys
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StatsController(ILogger<SurveyQuestionsController> logger, DataContext context) : ControllerBase
{
    // GET: api/Stats/GetBasicStats
    [HttpGet(nameof(GetBasicStats))]
    public async Task<BasicStats> GetBasicStats()
    {
        logger.LogInformation("Called GetBasicStats");

        var usersSurveyed = await context.SurveyResponses
            .Select(r => r.User.UserPrincipalName)
            .Distinct()
            .CountAsync();

        var usersResponded = await context.SurveyResponses
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
