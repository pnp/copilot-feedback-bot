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

    public Task<DateTime?> GetLastActivity<TReportDbType, TAbstractActivityRecord>(AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader, string forUPN)
        where TReportDbType : AbstractUsageActivityLog, new()
        where TAbstractActivityRecord : AbstractActivityRecord
    {
        var table = GetReportDbTypes(loader);
        var latest = table.Where(t => t.User.UserPrincipalName == forUPN).OrderByDescending(r=> r.DateOfActivity).Take(1);
        if (latest.Count() == 0)
        {
            return Task.FromResult<DateTime?>(null);
        }
        return Task.FromResult<DateTime?>(latest.First().DateOfActivity);
    }

    DbSet<TReportDbType> GetReportDbTypes<TReportDbType, TAbstractActivityRecord>(AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
            where TReportDbType : AbstractUsageActivityLog, new()
            where TAbstractActivityRecord : AbstractActivityRecord
    {
        var tableObj = _db.GetType().GetProperty(loader.DataContextPropertyName)?.GetValue(_db);
        if (tableObj == null)
        {
            throw new InvalidOperationException($"Table {typeof(TReportDbType).Name} not found in DataContext");
        }
        return (DbSet<TReportDbType>)tableObj;
    }

    /// <summary>
    /// Save to SQL. Needs a shared ConcurrentLookupDbIdsCache if running in parallel with other imports.
    /// </summary>
    public async Task SaveLoadedReports<TReportDbType, TAbstractActivityRecord>(Dictionary<DateTime, List<TAbstractActivityRecord>> LoadedReportPages,
        AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
            where TReportDbType : AbstractUsageActivityLog, new()
            where TAbstractActivityRecord : AbstractActivityRecord
    {
        var table = GetReportDbTypes(loader);

        int i = 0; var enUS = new System.Globalization.CultureInfo("en-US");
        var allInserts = new List<TReportDbType>();
        // For each day in dataset (Key)
        foreach (var dateTime in LoadedReportPages.Keys)
        {
            // Pre-cache all reports on that date
            _logger.LogDebug($"Saving {typeof(TReportDbType).Name} for date {dateTime.ToString("dd-MM-yyyy")}");



            var allReportsOnDate = await table.Where(t =>
                                            t.DateOfActivity.Year == dateTime.Year &&
                                            t.DateOfActivity.Month == dateTime.Month &&
                                            t.DateOfActivity.Day == dateTime.Day
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
                dateRequestedLog.DateOfActivity = dateTime.Date;

                loader.PopulateReportSpecificMetadata(dateRequestedLog, reportPage);

                i++;
            }
        }

        // All inserts at once
        table.AddRange(allInserts);

        await _db.SaveChangesAsync();
    }
}
