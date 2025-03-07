using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActivityImporter.Engine.Graph.O365UsageReports;

public interface IUsageReportPersistence
{
    Task<DateTime?> GetLastActivity<TReportDbType, TAbstractActivityRecord>(AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader, string forUPN)
        where TReportDbType : AbstractUsageActivityLog, new()
        where TAbstractActivityRecord : AbstractActivityRecord;
    Task SaveLoadedReports<TReportDbType, TAbstractActivityRecord>(Dictionary<DateTime, List<TAbstractActivityRecord>> reportPages, AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
        where TReportDbType : AbstractUsageActivityLog, new()
        where TAbstractActivityRecord : AbstractActivityRecord;
}