using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using Common.DataUtils;
using Entities.DB;
using Entities.DB.Entities;
using Entities.DB.LookupCaches.Discrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;


/// <summary>
/// Generic Graph report loader. Recursively loads and saves any Graph activity report.
/// </summary>
/// <typeparam name="TReportDbType">Type of EF table</typeparam>
/// <typeparam name="TPagableResponse">Type of report that's a pageable response</typeparam>
/// <typeparam name="TAbstractActivityRecord">Type of report page</typeparam>
public abstract class AbstractActivityLoader<TReportDbType, TAbstractActivityRecord>
    where TReportDbType : AbstractUsageActivityLog, new()
    where TAbstractActivityRecord : AbstractActivityRecord
{
    private readonly IUserActivityLoader _graphActivityLoader;
    private readonly IUsageReportPersistence _usageReportPersistence;

    internal AbstractActivityLoader(IUserActivityLoader graphActivityLoader, IUsageReportPersistence usageReportPersistence, ILogger telemetry)
    {
        _graphActivityLoader = graphActivityLoader;
        _usageReportPersistence = usageReportPersistence;
        Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
    }

    #region Props

    public abstract string ReportGraphURL { get; }

    public ILogger Telemetry { get; set; }

    public Dictionary<DateTime, List<TAbstractActivityRecord>> LoadedReportPages { get; set; } = new();

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

            Telemetry.LogInformation($"Loading {GetType().Name} for date {dt.ToString("dd-MM-yyyy")}");

            var dayReports = await _graphActivityLoader.LoadReport<TAbstractActivityRecord>(dt, ReportGraphURL);

            if (dayReports.Count > 0 && dayReports[0] is AbstractUserActivityUserRecord)
            {
                // Check if UPN is anonymous
                foreach (var reportPage in dayReports)
                {
                    var userRecord = reportPage as AbstractUserActivityUserRecord;
                    if (userRecord != null && !CommonStringUtils.IsEmail(userRecord.UPNFieldVal))
                    {
                        Telemetry.LogError($"Config Error: Usage reports have associated user email concealed - " +
                            $"we won't be able to link any activity back to users. " +
                            $"Please refer to the prerequisites documentation.");

                        // Don't save data
                        return;
                    }
                }
            }
            LoadedReportPages.Add(dt, dayReports);
        }
    }


    public async Task SaveLoadedReportsToSql(ConcurrentLookupDbIdsCache userEmailToDbIdCache, DataContext db, UserCache userCache)
    {
        await _usageReportPersistence.SaveLoadedReports(LoadedReportPages, this);
    }

    protected abstract long CountActivity(TAbstractActivityRecord activityPage);
    public abstract DbSet<TReportDbType> GetTable(DataContext context);
    public abstract void PopulateReportSpecificMetadata(TReportDbType newRecord, TAbstractActivityRecord activityPage);

    protected int GetOptionalInt(int? i)
    {
        if (i.HasValue)
        {
            return i.Value;
        }
        return 0;
    }
}
