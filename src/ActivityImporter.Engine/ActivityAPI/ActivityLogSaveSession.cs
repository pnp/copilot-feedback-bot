using ActivityImporter.Engine.ActivityAPI.Copilot;
using Common.DataUtils;
using Common.Engine.Config;
using Entities.DB;
using Entities.DB.Entities.AuditLog;
using Entities.DB.LookupCaches.Discrete;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace ActivityImporter.Engine.ActivityAPI;

/// <summary>
/// A class for saving a batch of ContentSets. Used
/// </summary>
public class ActivityLogSaveSession
{
    private CopilotAuditEventManager? _copilotEventResolver = null;
    private readonly GraphAppIndentityOAuthContext _authContext;
    private readonly AppConfig _appConfig;
    private readonly ILogger _logger;

    public ActivityLogSaveSession(DataContext db, AppConfig appConfig, ILogger logger)
    {
        Database = db;
        _appConfig = appConfig;
        _logger = logger;

        _authContext = new GraphAppIndentityOAuthContext(logger, appConfig.AuthConfig.ClientId, appConfig.AuthConfig.TenantId, appConfig.AuthConfig.ClientSecret, string.Empty, false);
        SiteCache = new SiteCache(db);
    }
    public SiteCache SiteCache { get; set; }
    public DataContext Database { get; set; }

    public Dictionary<Guid, SharePointEventMetadata> CachedSpEvents { get; set; } = new Dictionary<Guid, SharePointEventMetadata>();

    public CopilotAuditEventManager CopilotEventResolver => _copilotEventResolver ?? throw new Exception("Session not initialised");

    internal async Task Init()
    {
        await _authContext.InitClientCredential();
        var loader = new GraphFileMetadataLoader(new GraphServiceClient(_authContext.Creds), _logger);
        _copilotEventResolver = new CopilotAuditEventManager(_appConfig.ConnectionStrings.SQL, loader, _logger);
    }

    internal async Task CommitChanges()
    {
        // Save metadata updates done with EF
        Database.ChangeTracker.DetectChanges();
        await Database.SaveChangesAsync();

        await CopilotEventResolver.CommitAllChanges();
    }
}
