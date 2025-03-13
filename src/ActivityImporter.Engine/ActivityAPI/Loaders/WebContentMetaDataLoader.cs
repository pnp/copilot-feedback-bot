using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ActivityImporter.Engine.ActivityAPI.Loaders;

/// <summary>
/// Activity API implementation for ActivitySummaryLoader
/// </summary>
public class WebContentMetaDataLoader : ContentMetaDataLoader<ActivityReportInfo>
{
    private readonly string _tenantId;
    private readonly ConfidentialClientApplicationThrottledHttpClient _httpClient;

    public WebContentMetaDataLoader(string tenantId, ILogger telemetry, ConfidentialClientApplicationThrottledHttpClient httpClient) : base(telemetry)
    {
        _tenantId = tenantId;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Recursively get all metadata for an event query URL
    /// </summary>
    /// <returns>List of events</returns>
    protected override async Task<List<ActivityReportInfo>> LoadAllActivityReports(string auditContentType, TimePeriod chunk, int batchId)
    {
        // Build the uri to download 
        var metadataUri = $"https://manage.office.com/api/v1.0/{_tenantId}" +
            $"/activity/feed/subscriptions/content?ContentType={auditContentType}&PublisherIdentifier={_tenantId}&" +
            $"startTime={FormatDate(chunk.Start)}&endTime={FormatDate(chunk.End)}";

        var data = await DownloadMetadata(metadataUri, batchId);
#if DEBUG
        if (data.Count > 0)
        {
            Console.WriteLine($"DEBUG: GET METADATA {batchId}: {data.Count.ToString("N0")} change reports found between '{chunk.Start}'-'{chunk.End}'.");
        }
#endif
        return data;
    }

    /// <summary>
    /// Downloads change details for a change report, all pages
    /// </summary>
    public async Task<List<ActivityReportInfo>> DownloadMetadata(string changeReportUri, int batchId)
    {
        var responseMeta = new List<ActivityReportInfo>();

        // Try and download metadata content

        string? nextPageUri = null;
        const string NEXT_PAGE_PARAM = "NextPageUri";

        // Get this batch
        var response = await _httpClient.GetAsyncWithThrottleRetries(changeReportUri, _telemetry);

        // Read the content.  
        var responseFromServer = await response.Content.ReadAsStringAsync();
        try
        {
            response.EnsureSuccessStatusCode();

            // More data to get for events?
            if (response.Headers.Contains(NEXT_PAGE_PARAM))
            {
                nextPageUri = response.Headers.GetValues(NEXT_PAGE_PARAM).First();
            }
            else nextPageUri = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _telemetry.LogError($"Error downloading metadata {changeReportUri} with error '{ex.Message}'. If this happens every time, this may be an issue. Ignoring for now.");
#if DEBUG
            _telemetry.LogInformation("DEBUG: Response body was:\n" + responseFromServer);
#endif
        }

        // Do something with the response for this URL & the nextpage URL if needed
        if (!string.IsNullOrEmpty(responseFromServer))
        {
            // Deserialise the results from the HTTP response
            responseMeta = JsonConvert.DeserializeObject<List<ActivityReportInfo>>(responseFromServer) ?? new List<ActivityReportInfo>();

            // Add our own batch ID variable to each response
            foreach (var metaData in responseMeta)
            {
                metaData.BatchID = batchId;
            }

            // More data?
            if (!string.IsNullOrEmpty(nextPageUri))
            {
                // Add publisher to URL
                var nextPageURLWithPublisher = $"{nextPageUri}&PublisherIdentifier={_tenantId}";

                // Get next page
                var nextPage = await DownloadMetadata(nextPageURLWithPublisher, batchId);

                // Recursive call
                responseMeta.AddRange(nextPage);
            }

            return responseMeta;
        }
        else
        {
            responseMeta = new List<ActivityReportInfo>();
        }

        return responseMeta;
    }

    private string FormatDate(DateTime d)
    {
        // Activity API format: YYYY-MM-DDTHH:MM:SS
        string date = d.ToUniversalTime().ToString("yyyy-MM-dd");
        string time = d.ToUniversalTime().ToString("HH:mm:ss");
        return $"{date}T{time}";
    }
}
