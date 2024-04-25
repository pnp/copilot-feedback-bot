using ActivityImporter.Engine.ActivityAPI;
using ActivityImporter.Engine.ActivityAPI.Models;

namespace UnitTests.FakeLoaderClasses
{
    internal class FakeActivityReportPersistenceManager : IActivityReportPersistenceManager
    {
        public Task<ImportStat> CommitAll(ActivityReportSet activities)
        {
            //Console.WriteLine($"{nameof(FakeActivityReportPersistenceManager)}: Pretending to save {activities.Count} activities");

            return Task.FromResult(new ImportStat { Imported = activities.Count, Total = activities.Count });
        }
    }
}
