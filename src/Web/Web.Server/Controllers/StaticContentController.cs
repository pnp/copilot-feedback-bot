using Microsoft.AspNetCore.Mvc;

namespace Web.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class StaticContentController(ILogger<SurveyQuestionsController> logger) : ControllerBase
{
    // GET: StaticContent/Index
    [HttpGet(nameof(Index))]
    public async Task<IActionResult> Index([FromQuery] string resource)
    {
        logger.LogInformation("Called Index");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var content = await System.IO.File.ReadAllTextAsync(filePath);

        return Content(content, "text/html");
    }
}
