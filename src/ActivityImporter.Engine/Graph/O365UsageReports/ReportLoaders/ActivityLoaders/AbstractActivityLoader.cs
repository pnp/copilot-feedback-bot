using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using Common.DataUtils;
using Entities.DB;
using Entities.DB.Entities;
using Entities.DB.LookupCaches.Discrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders.ActivityLoaders
{

    /// <summary>
    /// Generic Graph report loader. Recursively loads and saves any Graph activity report.
    /// </summary>
    /// <typeparam name="TReportDbType">Type of EF table</typeparam>
    /// <typeparam name="TPagableResponse">Type of report that's a pageable response</typeparam>
    /// <typeparam name="TUserActivityUserRecord">Type of report page</typeparam>
    public abstract class AbstractActivityLoader<TReportDbType, TUserActivityUserRecord>
        where TReportDbType : AbstractUsageActivityLog, new()
        where TUserActivityUserRecord : AbstractActivityRecord
    {
        private readonly ManualGraphCallClient _client;

        internal AbstractActivityLoader(ManualGraphCallClient client, ILogger telemetry)
        {
            this._client = client;
            this.Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        #region Props

        public abstract string ReportGraphURL { get; }
        public abstract DbSet<TReportDbType> GetTable(DataContext context);

        public ILogger Telemetry { get; set; }

        public Dictionary<DateTime, List<TUserActivityUserRecord>> LoadedReportPages { get; set; } = new Dictionary<DateTime, List<TUserActivityUserRecord>>();

        #endregion


        public async Task PopulateLoadedReportPagesFromGraph(int daysBackMax)
        {
            // Activity reports don't tend to refresh until a couple of days late. Make sure we collect something useful. 
            if (daysBackMax < 3) daysBackMax = 3;
            else if (daysBackMax > 28) daysBackMax = 28;        // Also don't live for more than 28 days

            LoadedReportPages.Clear();

            for (int daysBackIdx = 0; daysBackIdx < daysBackMax; daysBackIdx++)
            {
                // Go back one extra day always. Otherwise we risk asking for data too soon...
                // Example: Message: {"error":{"code":"InvalidArgument","message":"Invalid date value specified: $DateTime.Now. Only support data for the past 28 days."}}
                var daysBack = (daysBackIdx + 1) * -1;
                var dt = DateTime.Now.AddDays(daysBack);

                Telemetry.LogInformation($"\nLoading {this.GetType().Name} for date {dt.ToString("dd-MM-yyyy")}");

                var requestUrl = $"{ReportGraphURL}(date={dt.ToString("yyyy-MM-dd")})?$format=application/json";
                var dayReports = await _client.LoadAllPagesWithThrottleRetries<TUserActivityUserRecord>(requestUrl, Telemetry);

                LoadedReportPages.Add(dt, dayReports);
            }
        }


        /// <summary>
        /// Save to SQL. Needs a shared ConcurrentLookupDbIdsCache if running in parallel with other imports.
        /// </summary>
        public async Task SaveLoadedReportsToSql(ConcurrentLookupDbIdsCache userEmailToDbIdCache, DataContext db, UserCache userCache)
        {
            int i = 0; var enUS = new System.Globalization.CultureInfo("en-US");
            var allInserts = new List<TReportDbType>();
            // For each day in dataset (Key)
            foreach (var dateTime in LoadedReportPages.Keys)
            {
                // Pre-cache all reports on that date
                var allReportsOnDate = await GetTable(db).Where(t =>
                                                t.Date.Year == dateTime.Year &&
                                                t.Date.Month == dateTime.Month &&
                                                t.Date.Day == dateTime.Day
                                            ).ToListAsync();

                // Look through Graph results & compare with already saved reports for this date
                foreach (var reportPage in LoadedReportPages[dateTime])
                {
                    // Do we have a cached ID for the lookup?
                    int? lookupId = null;
                    lookupId = userEmailToDbIdCache.GetCachedIdForName<TReportDbType>(reportPage.LookupFieldValue);

                    if (lookupId == null)
                    {
                        // See if there's already a log defined for this date + lookup (usually "user")
                        var lookup = await reportPage.GetOrCreateLookup(userCache);

                        // Sanity
                        if (!lookup.IsSavedToDB)
                        {
                            throw new InvalidOperationException("Cannot use unsaved lookups for activity records");
                        }

                        // Cache lookup
                        lookupId = lookup.ID;
                        userEmailToDbIdCache.AddOrUpdateForName<TReportDbType>(reportPage.LookupFieldValue, lookupId.Value);
                    }


                    var dateRequestedLog = allReportsOnDate.FirstOrDefault(t =>
                            t.AssociatedLookupId == lookupId.Value
                        );

                    // Output progress every 1000 imports
                    if (i > 0 && i % 1000 == 0)
                    {
                        Console.WriteLine($"{this.GetType().Name}: Saved {i} / {LoadedReportPages.SelectMany(r => r.Value).Count()}");
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
                            Telemetry.LogWarning($"Invalid LastActivity value: '{reportPage.LastActivityDateString}'");
                            dateRequestedLog.LastActivityDate = null;
                        }
                    }
                    PopulateReportSpecificMetadata(dateRequestedLog, reportPage);

                    i++;
                }
            }

            // All inserts at once
            GetTable(db).AddRange(allInserts);

            await db.SaveChangesAsync();
        }
        protected abstract long CountActivity(TUserActivityUserRecord activityPage);

        protected abstract void PopulateReportSpecificMetadata(TReportDbType newRecord, TUserActivityUserRecord activityPage);


        protected int GetOptionalInt(int? i)
        {
            if (i.HasValue)
            {
                return i.Value;
            }
            return 0;
        }
    }
}
