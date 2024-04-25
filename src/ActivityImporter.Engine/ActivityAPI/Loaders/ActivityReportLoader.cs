using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActivityImporter.Engine.ActivityAPI.Loaders;

/// <summary>
/// Loads activity data from the Activity API
/// https://learn.microsoft.com/en-us/office/office-365-management-api/office-365-management-activity-api-reference
/// </summary>
public class ActivityReportWebLoader : IActivityReportLoader<ActivityReportInfo>
{
    private AutoThrottleHttpClient _httpClient;
    private readonly ILogger _telemetry;
    private readonly string _tenantId;
    public ActivityReportWebLoader(AutoThrottleHttpClient httpClient, ILogger telemetry, string tenantId)
    {
        _httpClient = httpClient;
        _telemetry = telemetry;
        _tenantId = tenantId;
    }

    /// <summary>
    /// Load full activity reports from summary links
    /// </summary>
    public async Task<ActivityReportSet> Load(ActivityReportInfo metadata)
    {
        // Apply the PublisherIdentifier value as a parameter to each audit event fetch from the API
        var newUri = metadata.ContentUri.ToString() + "?PublisherIdentifier=" + _tenantId;

        // Get the HTTP response from Activity API. Block if a reauth request is happening though. 
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.GetAsyncWithThrottleRetries(newUri, _telemetry);
        }
        catch (HttpRequestException ex)
        {
            _telemetry.LogInformation($"Got error '{ex.Message}' downloading {metadata.ContentUri}. Will try again on next cycle.");
            return new WebActivityReportSet();
        }

        // Otherwise parse response
        var jSonBody = await response.Content.ReadAsStringAsync();

        var logs = new WebActivityReportSet();

        // A report download can have multiple reports in a Json array.
        JArray allReportsData;
        try
        {
            allReportsData = JArray.Parse(jSonBody);
        }
        catch (JsonReaderException ex)
        {
            _telemetry.LogError($"Failed to parse JSON from {metadata.ContentUri}. Error: {ex.Message}");
            return logs;
        }

        var reportsArray = allReportsData.Children();

        var unknownWorkloads = new List<string>();
        foreach (var reportItem in reportsArray)
        {
            var logJson = reportItem.ToString();
            var logBase = JsonConvert.DeserializeObject<WorkloadOnlyAuditLogContent>(logJson) ?? new WorkloadOnlyAuditLogContent() { Workload = "Unknown" };
            AbstractAuditLogContent? thisAuditLogReport = null;

            // Determine which deserialization to use, depending on the workload
            if (logBase.Workload == ActivityImportConstants.WORKLOAD_SP || logBase.Workload == ActivityImportConstants.WORKLOAD_OD)
            {
                thisAuditLogReport = JsonConvert.DeserializeObject<SharePointAuditLogContent>(logJson);
            }
            else if (logBase.Workload == ActivityImportConstants.WORKLOAD_COPILOT)
            {
                try
                {
                    thisAuditLogReport = JsonConvert.DeserializeObject<CopilotAuditLogContent>(logJson);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"Failed to deserialize Copilot log: {logJson}");
                    throw;
                }
            }
            else
            {
                if (!unknownWorkloads.Contains(logBase.Workload))
                {
                    Console.WriteLine($"Unknown workload '{logBase.Workload}' in activity API. Ignoring.");
                    unknownWorkloads.Add(logBase.Workload);
                }
            }

            if (thisAuditLogReport != null)
            {
                logs.Add(thisAuditLogReport);
            }
        }

        logs.OriginalMetadata = metadata;
        foreach (var log in logs)
        {
            // Save original file content for each item, so we don't have to associated a content-set on save
            log.OriginalImportFileContents = jSonBody;
        }

        return logs;
    }
}
