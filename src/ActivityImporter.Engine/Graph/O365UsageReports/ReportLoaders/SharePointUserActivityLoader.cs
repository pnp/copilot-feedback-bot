using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using Entities.DB;
using Entities.DB.Entities.UsageReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;

// https://docs.microsoft.com/en-us/graph/api/reportroot-getonedriveactivityuserdetail?view=graph-rest-beta
public class SharePointUserActivityLoader : AbstractActivityLoader<SharePointUserActivityLog, SharePointUserActivityRecord>
{
    public SharePointUserActivityLoader(ManualGraphCallClient client, ILogger logger)
        : base(client, logger)
    {
    }

    protected override void PopulateReportSpecificMetadata(SharePointUserActivityLog todaysLog, SharePointUserActivityRecord userActivityReportPage)
    {
        todaysLog.SharedInternally = userActivityReportPage.SharedInternally;
        todaysLog.SharedExternally = userActivityReportPage.SharedExternally;
        todaysLog.Synced = userActivityReportPage.Synced;
        todaysLog.ViewedOrEdited = userActivityReportPage.ViewedOrEdited;
        todaysLog.LastActivityDate = userActivityReportPage.LastActivityDate;
    }

    protected override long CountActivity(SharePointUserActivityRecord activityPage)
    {
        if (activityPage is null)
        {
            throw new ArgumentNullException(nameof(activityPage));
        }

        long count = 0;

        count += activityPage.SharedInternally;
        count += activityPage.SharedExternally;
        count += activityPage.Synced;
        count += activityPage.ViewedOrEdited;

        return count;
    }
    public override string ReportGraphURL => "https://graph.microsoft.com/beta/reports/getSharePointActivityUserDetail";

    public override DbSet<SharePointUserActivityLog> GetTable(DataContext context) => context.SharePointUserActivityLogs;
}
