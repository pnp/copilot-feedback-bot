using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using Entities.DB.Entities;

namespace ActivityImporter.Engine.Graph.O365UsageReports;

public interface IUsageReportPersistence
{
    Task SaveLoadedReports<TReportDbType, TAbstractActivityRecord>(Dictionary<DateTime, List<TAbstractActivityRecord>> reportPages, AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
        where TReportDbType : AbstractUsageActivityLog, new()
        where TAbstractActivityRecord : AbstractActivityRecord;
}