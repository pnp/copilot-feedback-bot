using ActivityImporter.Engine.Graph.O365UsageReports;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using UnitTests.FakeLoaderClasses;

namespace UnitTests;

[TestClass]
public class UsageLoaderTests : AbstractTest
{

    [TestMethod]
    public async Task FakeTeamsActivity()
    {
        var activityLoader = new FakeUserActivityLoader();
        var loader = new TeamsUserDeviceLoader(activityLoader, new SqlUsageReportPersistence(_db, _logger), _logger);
        var p = await loader.LoadAndSaveUsagePages();
        Assert.AreEqual(3, p.Count);

    }
}
