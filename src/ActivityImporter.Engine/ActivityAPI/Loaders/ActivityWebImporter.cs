using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils;
using Common.DataUtils.Http;
using Common.Engine.Config;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.ActivityAPI.Loaders;


/// <summary>
/// Web-loading activity importer
/// </summary>
public class ActivityWebImporter : ActivityImporter<ActivityReportInfo>
{
    private ActivityReportWebLoader _activityReportWebLoader;
    private WebContentMetaDataLoader _contentMetaDataLoader;
    private ActivitySubscriptionManager _activitySubscriptionManager;
    private readonly AzureADAuthConfig _authConfig;

    public ActivityWebImporter(AzureADAuthConfig authConfig, ILogger telemetry, int maxSavesPerBatch) : base(telemetry, maxSavesPerBatch)
    {
        var auth = new ActivityAPIAppIndentityOAuthContext(telemetry, authConfig.ClientId, authConfig.TenantId, authConfig.ClientSecret, string.Empty, false);
        var httpClient = new ConfidentialClientApplicationThrottledHttpClient(auth, false, telemetry);
        _activityReportWebLoader = new ActivityReportWebLoader(httpClient, telemetry, authConfig.TenantId);
        _contentMetaDataLoader = new WebContentMetaDataLoader(authConfig.TenantId, telemetry, httpClient);
        _activitySubscriptionManager = new ActivitySubscriptionManager(authConfig.TenantId, telemetry, httpClient, ActivityImportConstants.ACTIVITY_CONTENT_TYPES);
        _authConfig = authConfig;
    }


    /// <summary>
    /// Unit tests constructors
    /// </summary>
    public ActivityWebImporter(AzureADAuthConfig authConfig, ConfidentialClientApplicationThrottledHttpClient httpClient, ILogger telemetry, int maxSavesPerBatch) : base(telemetry, maxSavesPerBatch)
    {
        _activityReportWebLoader = new ActivityReportWebLoader(httpClient, telemetry, authConfig.TenantId);
        _contentMetaDataLoader = new WebContentMetaDataLoader(authConfig.TenantId, telemetry, httpClient);
        _activitySubscriptionManager = new ActivitySubscriptionManager(authConfig.TenantId, telemetry, httpClient, ActivityImportConstants.ACTIVITY_CONTENT_TYPES);
        _authConfig = authConfig;
    }
    public ActivityWebImporter(AzureADAuthConfig authConfig, ConfidentialClientApplicationThrottledHttpClient fakeClient, ILogger telemetry) :
        this(authConfig, fakeClient, telemetry, 1)
    {
    }


    public override IActivityReportLoader<ActivityReportInfo> ReportLoader => _activityReportWebLoader;
    public override ContentMetaDataLoader<ActivityReportInfo> ContentMetaDataLoader => _contentMetaDataLoader;

    public override IActivitySubscriptionManager ActivitySubscriptionManager => _activitySubscriptionManager;
}
