using Common.DataUtils;
using Entities.DB.LookupCaches.Discrete;
using Entities.DB;
using Entities.DB.Entities;
using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports;

public class SqlUsageReportPersistence : IUsageReportPersistence
{
    private readonly ConcurrentLookupDbIdsCache _userEmailToDbIdCache;
    private readonly DataContext _db;
    private readonly UserCache _userCache;
    private readonly ILogger _logger;

    public SqlUsageReportPersistence(ConcurrentLookupDbIdsCache userEmailToDbIdCache, DataContext db, UserCache userCache, ILogger logger)
    {
        _userEmailToDbIdCache = userEmailToDbIdCache;
        _db = db;
        _userCache = userCache;
        _logger = logger;
    }

    /// <summary>
    /// Save to SQL. Needs a shared ConcurrentLookupDbIdsCache if running in parallel with other imports.
    /// </summary>
    public async Task SaveLoadedReports<TReportDbType, TAbstractActivityRecord>(Dictionary<DateTime, List<TAbstractActivityRecord>> LoadedReportPages,
        AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
            where TReportDbType : AbstractUsageActivityLog, new()
            where TAbstractActivityRecord : AbstractActivityRecord
    {
        int i = 0; var enUS = new System.Globalization.CultureInfo("en-US");
        var allInserts = new List<TReportDbType>();
        // For each day in dataset (Key)
        foreach (var dateTime in LoadedReportPages.Keys)
        {
            // Pre-cache all reports on that date
            _logger.LogDebug($"Saving {typeof(TReportDbType).Name} for date {dateTime.ToString("dd-MM-yyyy")}");
            var allReportsOnDate = await loader.GetTable(_db).Where(t =>
                                            t.Date.Year == dateTime.Year &&
                                            t.Date.Month == dateTime.Month &&
                                            t.Date.Day == dateTime.Day
                                        ).ToListAsync();

            // Look through Graph results & compare with already saved reports for this date
            foreach (var reportPage in LoadedReportPages[dateTime])
            {
                // Do we have a cached ID for the lookup?
                int? lookupId = null;
                lookupId = _userEmailToDbIdCache.GetCachedIdForName<TReportDbType>(reportPage.LookupFieldValue);

                if (lookupId == null)
                {
                    // See if there's already a log defined for this date + lookup (usually "user")
                    var lookup = await reportPage.GetOrCreateLookup(_userCache);

                    // Sanity
                    if (!lookup.IsSavedToDB)
                    {
                        throw new InvalidOperationException("Cannot use unsaved lookups for activity records");
                    }

                    // Cache lookup
                    lookupId = lookup.ID;
                    _userEmailToDbIdCache.AddOrUpdateForName<TReportDbType>(reportPage.LookupFieldValue, lookupId.Value);
                }


                var dateRequestedLog = allReportsOnDate.FirstOrDefault(t =>
                        t.AssociatedLookupId == lookupId.Value
                    );

                // Output progress every 1000 imports
                if (i > 0 && i % 1000 == 0)
                {
                    Console.WriteLine($"{GetType().Name}: Saved {i} / {LoadedReportPages.SelectMany(r => r.Value).Count()}");
                }

                // Create new log if necesary
                if (dateRequestedLog == null)
                {
                    dateRequestedLog = new TReportDbType()
                    {
                        AssociatedLookupId = lookupId.Value   // date set below
                    };

                    // Add new logs to list to insert
                    allInserts.Add(dateRequestedLog);
                }

                // Set log stats
                dateRequestedLog.Date = dateTime.Date;

                // Example: "2017-08-30"
                var activityDate = DateTime.MinValue;
                if (!string.IsNullOrEmpty(reportPage.LastActivityDateString))
                {
                    if (DateTime.TryParseExact(reportPage.LastActivityDateString, "yyyy-MM-dd", enUS, System.Globalization.DateTimeStyles.None, out activityDate))
                    {
                        dateRequestedLog.LastActivityDate = activityDate;
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid LastActivity value: '{reportPage.LastActivityDateString}'");
                        dateRequestedLog.LastActivityDate = null;
                    }
                }
                loader.PopulateReportSpecificMetadata(dateRequestedLog, reportPage);

                i++;
            }
        }

        // All inserts at once
        loader.GetTable(_db).AddRange(allInserts);

        await _db.SaveChangesAsync();
    }
}
