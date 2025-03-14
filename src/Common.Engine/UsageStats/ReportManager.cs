using Entities.DB.Models;
using Microsoft.Extensions.Logging;

namespace Common.Engine.UsageStats;

public class ReportManager(IUsageDataLoader loader, ILogger<ReportManager> logger)
{
    public async Task<UsageStatsReport> GetReport(LoaderUsageStatsReportFilter filter)
    {
        logger.LogInformation("Loading usage stats report with filter: {@filter}", filter);
        var data = await loader.LoadRecords(filter);
        return new UsageStatsReport(data, filter);
    }
}

public interface IUsageDataLoader
{
    Task<IEnumerable<IActivitiesWeeklyRecord>> LoadRecords(LoaderUsageStatsReportFilter filter);
    Task RefreshProfilingStats(int weeksToKeep);
    Task ClearProfilingStats();
}


public class LoaderUsageStatsReportFilter : UsageStatsReportFilter
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

