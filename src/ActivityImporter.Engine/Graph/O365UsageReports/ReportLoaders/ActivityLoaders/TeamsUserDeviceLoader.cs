using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using Entities.DB;
using Entities.DB.Entities.UsageReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders.ActivityLoaders;


/// <summary>
/// https://docs.microsoft.com/en-us/graph/api/reportroot-getteamsdeviceusageuserdetail?view=graph-rest-beta
/// </summary>
public class TeamsUserDeviceLoader : AbstractActivityLoader<GlobalTeamsUserDeviceUsageLog, TeamsDeviceUsageUserDetail>
{
    public TeamsUserDeviceLoader(ManualGraphCallClient client, ILogger telemetry)
        : base(client, telemetry)
    {
    }

    public override string ReportGraphURL => "https://graph.microsoft.com/beta/reports/getTeamsDeviceUsageUserDetail";

    protected override void PopulateReportSpecificMetadata(GlobalTeamsUserDeviceUsageLog dateRequestedLog, TeamsDeviceUsageUserDetail reportPage)
    {
        dateRequestedLog.UsedAndroidPhone = reportPage.UsedAndroidPhone;
        dateRequestedLog.UsedIOS = reportPage.UsediOS;
        dateRequestedLog.UsedMac = reportPage.UsedMac;
        dateRequestedLog.UsedWeb = reportPage.UsedWeb;
        dateRequestedLog.UsedWindows = reportPage.UsedWindows;
        dateRequestedLog.UsedWindowsPhone = reportPage.UsedWindowsPhone;
        dateRequestedLog.UsedLinux = reportPage.UsedLinux;
        dateRequestedLog.UsedChromeOS = reportPage.UsedChromeOs;
    }

    protected override long CountActivity(TeamsDeviceUsageUserDetail activityPage)
    {
        if (activityPage is null)
        {
            throw new ArgumentNullException(nameof(activityPage));
        }

        long count = 0;
        if (activityPage.UsedAndroidPhone.HasValue && activityPage.UsedAndroidPhone.Value) count++;
        if (activityPage.UsediOS.HasValue && activityPage.UsediOS.Value) count++;
        if (activityPage.UsedMac.HasValue && activityPage.UsedMac.Value) count++;
        if (activityPage.UsedWeb.HasValue && activityPage.UsedWeb.Value) count++;
        if (activityPage.UsedWindows.HasValue && activityPage.UsedWindows.Value) count++;
        if (activityPage.UsedWindowsPhone.HasValue && activityPage.UsedWindowsPhone.Value) count++;
        if (activityPage.UsedLinux.HasValue && activityPage.UsedLinux.Value) count++;
        if (activityPage.UsedChromeOs.HasValue && activityPage.UsedChromeOs.Value) count++;

        return count;
    }
    public override DbSet<GlobalTeamsUserDeviceUsageLog> GetTable(DataContext context) => context.TeamsUserDeviceUsageLog;
}