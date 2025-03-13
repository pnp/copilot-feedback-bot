using Common.Engine.Notifications;
using Common.Engine.Surveys;
using Entities.DB;
using Microsoft.AspNetCore.Mvc;

namespace Web.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TriggersController(SurveyManager surveyManager, IBotConvoResumeManager botConvoResumeManager, DataContext context, ILogger<TriggersController> logger) : ControllerBase
{
    private readonly SurveyManager _surveyManager = surveyManager;
    private readonly IBotConvoResumeManager _botConvoResumeManager = botConvoResumeManager;
    private readonly DataContext _context = context;
    private readonly ILogger<TriggersController> _logger = logger;

    // Send surveys to all users that have new survey events, installing bot for users that don't have it
    // POST: api/Triggers/SendSurveys
    [HttpPost(nameof(SendSurveys))]
    public async Task<IActionResult> SendSurveys()
    {
        var sent = await _surveyManager.FindAndProcessNewSurveyEventsAllUsers();
        return Ok($"Sent {sent} new surveys");
    }

    // Force install bot for a user, regardless of whether they have any copilot activity or not
    // POST: api/Triggers/InstallBotForUser
    [HttpPost(nameof(InstallBotForUser))]
    public async Task<IActionResult> InstallBotForUser(string upn)
    {
        await _botConvoResumeManager.ResumeConversation(upn);
        return Ok($"Bot installed for user {upn}");
    }

    // POST: api/Triggers/GenerateFakeActivityFor
    [HttpPost(nameof(GenerateFakeActivityFor))]
    public async Task<IActionResult> GenerateFakeActivityFor(string upn)
    {
        await FakeDataGen.GenerateFakeCopilotFor(upn, _context, _logger);
        await FakeDataGen.GenerateFakeOfficeActivityFor(upn, DateTime.Now, _context, _logger);
        await _context.SaveChangesAsync();
        return Ok($"Generated fake data for {upn}");
    }


    // POST: api/Triggers/GenerateFakeActivityForAllUsers
    [HttpPost(nameof(GenerateFakeActivityForAllUsers))]
    public async Task<IActionResult> GenerateFakeActivityForAllUsers()
    {
        await FakeDataGen.GenerateFakeActivityForAllUsers(_context, _logger);

        return Ok($"Generated fake data for all users");
    }
}
