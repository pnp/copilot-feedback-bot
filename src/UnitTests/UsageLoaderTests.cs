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
        var loader = new TeamsUserDeviceLoader(activityLoader, new FakeUsageReportPersistence(), _logger);
        await loader.PopulateLoadedReportPagesFromGraph(3);
        Assert.AreEqual(3, loader.LoadedReportPages.Count);

    }
}
