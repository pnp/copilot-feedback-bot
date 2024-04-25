using ActivityImporter.Engine.ActivityAPI.Models;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.ActivityAPI.Loaders;

/// <summary>
/// Loads activity summary objects for a given time-period.
/// </summary>
public abstract class ContentMetaDataLoader<SUMMARYTYPE>
{
    protected readonly ILogger _telemetry;

    protected ContentMetaDataLoader(ILogger telemetry)
    {
        _telemetry = telemetry;
    }

    /// <summary>
    /// Load all summaries for a specific content type & time.
    /// </summary>
    protected abstract Task<List<SUMMARYTYPE>> LoadAllActivityReports(string auditContentType, TimePeriod chunk, int batchId);

    /// <summary>
    /// Enumerates the period of time were retrieving metadata for bearing in mind the configuration
    /// and the maximum chunk size and earliest date supported by the API
    /// </summary>
    public List<TimePeriod> GetScanningTimeChunksFromNow(int daysBeforeNowToDownload)
    {
        var daysToAdd = -1;
        if (daysBeforeNowToDownload > 1)
        {
            daysToAdd = daysBeforeNowToDownload * -1;
        }
        var extractStart = DateTime.UtcNow.AddDays(daysToAdd);
        return TimePeriod.GetScanningTimeChunksFrom(extractStart, DateTime.UtcNow);
    }

    /// <summary>
    /// Fetch all the metadata from the service in time chunk sized peices, but return it as a single stream.
    /// It will request metadata for the next time chunk asychronously while the prevoious one is being processed.
    /// Sometimes a single time chunk will come back in pages requiring several loops
    /// </summary>
    public async Task<List<SUMMARYTYPE>> GetChangesSummary(int daysBeforeNowToDownload, List<string> active)
    {
        // Request URL template
        // Reference: https://msdn.microsoft.com/en-us/office-365/office-365-management-activity-api-reference

        // Get time chunks we need to download
        var timeChunks = GetScanningTimeChunksFromNow(daysBeforeNowToDownload);

        var allResults = new List<SUMMARYTYPE>();

        if (timeChunks.Count == 0)
        {
            _telemetry.LogInformation("Audit events import: ERROR: Could not download activity - no time-chunks for activity scanning using configured values.");
        }
        else
        {
            // https://msdn.microsoft.com/en-us/office-365/office-365-management-activity-api-reference
            _telemetry.LogInformation($"Audit events import: getting changes summary from Office 365 Activity API from '{timeChunks.First().Start}' to '{timeChunks.Last().End}'...");

            int batchId = 0;
            var downloadListThreads = new List<Task<List<SUMMARYTYPE>>>();

            // For every valid content type in the configuration
            foreach (var auditContentType in active)
            {
                // For every time chunk we need
                foreach (var chunk in timeChunks)
                {
                    batchId++;

                    // Create new downloader async
                    var loaderThread = LoadAllActivityReports(auditContentType, chunk, batchId);

                    // Add task to list to wait for
                    downloadListThreads.Add(loaderThread);
                }
            }

            // Wait for all the selects to finish
            await Task.WhenAll(downloadListThreads);

            // Combine results
            foreach (var t in downloadListThreads)
            {
                if (t.Result.Count > 0)
                {
                    allResults.AddRange(t.Result);
                }
            }
        }

        return allResults;
    }
}
