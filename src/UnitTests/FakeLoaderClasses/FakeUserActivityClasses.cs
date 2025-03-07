using ActivityImporter.Engine.Graph.O365UsageReports;
using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using Common.DataUtils;
using Entities.DB.Entities;

namespace UnitTests.FakeLoaderClasses;

public class FakeUserActivityLoader : IUserActivityLoader
{
    public Task<List<TAbstractActivityRecord>> LoadReport<TAbstractActivityRecord>(DateTime dt, string reportGraphURL) 
        where TAbstractActivityRecord : AbstractActivityRecord
    {
        var datoir = new List<TeamsDeviceUsageUserDetail>
        {
            new TeamsDeviceUsageUserDetail
            {
                LastActivityDateString = DateTime.UtcNow.AddDays(-1).ToGraphDateString(),
                UserPrincipalName = "user@org.local"
            }
        };

        return Task.FromResult(datoir.Cast<TAbstractActivityRecord>().ToList());
    }
}

public class FakeUsageReportPersistence : IUsageReportPersistence
{
    public Task SaveLoadedReports<TReportDbType, TAbstractActivityRecord>(Dictionary<DateTime, List<TAbstractActivityRecord>> reportPages, AbstractActivityLoader<TReportDbType, TAbstractActivityRecord> loader)
        where TReportDbType : AbstractUsageActivityLog, new()
        where TAbstractActivityRecord : AbstractActivityRecord
    {
        throw new NotImplementedException();
    }
}