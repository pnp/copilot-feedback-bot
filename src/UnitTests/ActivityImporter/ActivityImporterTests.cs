using ActivityImporter.Engine.ActivityAPI;
using ActivityImporter.Engine.ActivityAPI.Models;
using UnitTests.FakeLoaderClasses;

namespace UnitTests.ActivityImporter;

[TestClass]
public class ActivityImporterTests : AbstractTest
{
    /// <summary>
    /// Check with in-memory adaptors so we can test save logic with various data ranges quickly
    /// </summary>
    //[TestMethod]
    public async Task FakeInMemoryImportTests()
    {
        var saveManager = new FakeActivityReportPersistenceManager();
        for (int i = 1; i < 1000; i++)
        {
            var testLoader = new FakeActivityImporter(i, new Entities.DB.Entities.User(), _logger);
            var multiplier = testLoader.ContentMetaDataLoader.GetScanningTimeChunksFromNow(ActivityImporter<BaseActivityReportInfo>.MAX_DAYS_TO_DOWNLOAD).Count;

            var r = await testLoader.LoadReportsAndSave(saveManager);
            var expected = i * multiplier;
            Assert.AreEqual(r.Total, expected);
        }
    }

    /// <summary>
    /// Check the SQL adaptor doesn't throw exceptions for some basic tests at least
    /// </summary>
    [TestMethod]
    public async Task ActivityReportSqlPersistenceManagerTests()
    {
        var sqlAdaptor = new ActivityReportSqlPersistenceManager(_db, _config, new UnitTestingAllowAllFilterConfig(), _logger);

        var randoString = Guid.NewGuid().ToString();
        var firstUser = new Entities.DB.Entities.User { AzureAdId = randoString, UserPrincipalName = randoString };
        _db.Users.Add(firstUser);
        await _db.SaveChangesAsync();

        var testLoader = new FakeActivityImporter(10, firstUser, _logger);
        await testLoader.LoadReportsAndSave(sqlAdaptor);
    }

    class UnitTestingAllowAllFilterConfig : AuditFilterConfig
    {
    }
}
