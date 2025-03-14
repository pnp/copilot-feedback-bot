using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils;
using Common.DataUtils.Sql.Inserts;
using Common.Engine.Config;
using Entities.DB.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;

namespace ActivityImporter.Engine.ActivityAPI;

/// <summary>
/// SQL adaptor for saving activity reports. 
/// Saves to a staging table, merges everything with a SQL script, then processes workload specific metadata updates seperately.
/// </summary>
public class ActivityReportSqlPersistenceManager : IActivityReportPersistenceManager
{
    private readonly DataContext _db;
    private readonly AppConfig _appConfig;
    private readonly AuditFilterConfig _filterConfig;
    private readonly ILogger _telemetry;

    private static SemaphoreSlim _sqlSaveSemaphore = new SemaphoreSlim(1);      // Make sure we're only saving one thread at a time

    public ActivityReportSqlPersistenceManager(DataContext db, AppConfig appConfig, AuditFilterConfig filterConfig, ILogger telemetry)
    {
        _db = db;
        _appConfig = appConfig;
        _filterConfig = filterConfig;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Write all to SQL with a new data cache for the events only in activities content-set
    /// </summary>
    public async Task<ImportStat> CommitAll(ActivityReportSet activities)
    {
        if (activities.Count > 0)
        {
            var cache = ActivityImportCache.GetAndBuildNewCache(_db, activities.OldestContent, activities.NewestContent);

            return await CommitAllToSQL(activities, cache);
        }
        else return new ImportStat();
    }

    /// <summary>
    /// Write all to SQL with an existing cache
    /// </summary>
    async Task<ImportStat> CommitAllToSQL(ActivityReportSet activities, ActivityImportCache cache)
    {
#if DEBUG
        Console.WriteLine($"DEBUG: Processing {activities.Count.ToString("n0")} activity reports...");
#endif
        var allStats = new ImportStat();

        // Allow only one save at a time otherwise we'll get errors when we try and create the temp table without clearing it down 1st
        await _sqlSaveSemaphore.WaitAsync();

        // Create our own connection & context to use it
        try
        {
            using (var con = new SqlConnection(_appConfig.ConnectionStrings.SQL))
            {
                con.Open();

                var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                optionsBuilder.UseSqlServer(con);
                using (var db = new DataContext(optionsBuilder.Options))
                {
                    // Add all activity data to staging table
                    var stats = await SaveToSqlAllTheThings(activities, _appConfig.ConnectionStrings.SQL, cache);
                    allStats.AddStats(stats);
                }
            }
        }
        finally
        {
            _sqlSaveSemaphore.Release();        // Whatever happens, make sure we release the semaphore 
        }

        return allStats;
    }

    /// <summary>
    /// Fill up staging table & return import result. Need our own connection & context to use it
    /// </summary>
    private async Task<ImportStat> SaveToSqlAllTheThings(ActivityReportSet activities, string connectionString, ActivityImportCache cache)
    {
        var listOfActivitiesSavedToSQL = new ConcurrentBag<AbstractAuditLogContent>();
        var logsToInsert = new InsertBatch<SPAuditLogTempEntity>(connectionString, _telemetry);
        var processedIds = new ConcurrentBag<Guid>();
        var stats = new ImportStat() { Total = activities.Count };

        foreach (var abtractLog in activities)
        {
            // Don't insert duplicates in same set
            if (!processedIds.Contains(abtractLog.Id) && !cache.HaveSeenInProcessedOrIgnoredEvents(abtractLog))
            {
                var result = SaveResultEnum.NotSaved;
                if (_filterConfig.InScope(abtractLog))
                {
                    var userNameOrHash = abtractLog.UserId;
                    logsToInsert.Rows.Add(new SPAuditLogTempEntity(abtractLog, userNameOrHash));

                    // Remember we've done this one now
                    cache.RememberProcessedEvent(abtractLog);
                    result = SaveResultEnum.Imported;
                }
                else
                {
                    // No URL
                    cache.RememberNewlyIgnoredEvent(abtractLog);
                    result = SaveResultEnum.OutOfScope;
                }

                // Update stats
                if (result == SaveResultEnum.Imported)
                {
                    stats.Imported++;
                    listOfActivitiesSavedToSQL.Add(abtractLog);
                }
                else if (result == SaveResultEnum.ProcessedAlready) stats.ProcessedAlready++;
                else if (result == SaveResultEnum.OutOfScope) stats.URLsOutOfScope++;
                else throw new InvalidOperationException("Unexpected save result");

                processedIds.Add(abtractLog.Id);
            }
        }

        // Merge data
#if DEBUG
        Console.WriteLine("\nDEBUG: Merging activity staging table...");
#endif
        // Merge to normal tables

        var rr = new ProjectResourceReader(System.Reflection.Assembly.GetExecutingAssembly());
        var sqlOriginal = rr.ReadResourceString("ActivityImporter.Engine.ActivityAPI.Copilot.SQL.insert_activity_from_staging_table.sql");

        var mergeSQL = sqlOriginal.Replace("${STAGING_TABLE_ACTIVITY}", ActivityImportConstants.STAGING_TABLE_ACTIVITY_SP);
        await logsToInsert.SaveToStagingTable(mergeSQL);

        #region Add Extra Metadata

        // Add metadata the traditional way with EF. By now should have all the sites saved. 
        var saveSession = new ActivityLogSaveSession(_db, _appConfig, _telemetry);
        await saveSession.Init();
        int metaSaveIdx = 0, changesMadeCount = 0;
#if DEBUG
        Console.WriteLine($"\nDEBUG: Updating metadata for {listOfActivitiesSavedToSQL.Count.ToString("n0")} saved events...");
#endif
        if (listOfActivitiesSavedToSQL.Count > 0)
        {
            var ids = listOfActivitiesSavedToSQL.Select(l => l.Id).ToList();
            var eventsJustSaved = _db.AuditEventsCommon
                .Include(e => e.User)
                .Where(e => ids.Contains(e.Id)).ToList();

            var spEventsJustSaved = _db.SharePointEvents
                .Include(spe => spe.AuditEvent)
                .Where(e => ids.Contains(e.AuditEventID)).ToList();

            // Hack: Add to cache so we can use it in the metadata update.
            // We do this with SP events because of the number of lookups involved, for which the insert script takes care of
            foreach (var e in spEventsJustSaved)
            {
                saveSession.CachedSpEvents.Add(e.AuditEventID, e);
            }

            foreach (var log in listOfActivitiesSavedToSQL)
            {
#if DEBUG
                if (metaSaveIdx > 0 && metaSaveIdx % 1000 == 0)
                {
                    float percentDone = metaSaveIdx / (float)listOfActivitiesSavedToSQL.Count * 100;
                    Console.Write($"{Math.Round(percentDone, 0)}%...");
                }
#endif
                // Add metadata
                var eventSaved = eventsJustSaved.Where(e => e.Id == log.Id).SingleOrDefault();
                if (eventSaved != null)
                {
                    var changesMade = await log.ProcessExtendedProperties(saveSession, eventSaved);
                    if (changesMade)
                        changesMadeCount++;
                }

                metaSaveIdx++;
            }
        }
#if DEBUG
        Console.WriteLine($"DEBUG: Updated metadata for {changesMadeCount.ToString("n0")} saved events");
#endif

        await saveSession.CommitChanges();


        #endregion

        return stats;
    }
}
