using Azure.Data.Tables;
using Common.Engine.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AppInfoController(ILogger<AppInfoController> logger, AppConfig config) : ControllerBase
{
    // POST: api/AppInfo/GetClientConfig
    [HttpPost(nameof(GetClientConfig))]
    public IActionResult GetClientConfig()
    {
        logger.LogInformation("Called GetClientConfig");
        var client = new TableServiceClient(config.ConnectionStrings.Storage);

        // Generate a new shared-access-signature
        var sasUri = client.GenerateSasUri(Azure.Data.Tables.Sas.TableAccountSasPermissions.Read, Azure.Data.Tables.Sas.TableAccountSasResourceTypes.Service,
            DateTime.Now.AddDays(1));

        // Return for react app
        return Ok(new ServiceConfiguration
        {
            StorageInfo = new StorageInfo
            {
                AccountURI = client.Uri.ToString(),
                SharedAccessToken = sasUri.Query
            }
        });
    }

}

internal class StorageInfo
{
    public required string AccountURI { get; set; }
    public required string SharedAccessToken { get; set; }
}

internal class ServiceConfiguration
{
    public required StorageInfo StorageInfo { get; set; }
}
