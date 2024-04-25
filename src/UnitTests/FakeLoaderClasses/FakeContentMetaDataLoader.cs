using ActivityImporter.Engine.ActivityAPI.Loaders;
using ActivityImporter.Engine.ActivityAPI.Models;
using Microsoft.Extensions.Logging;

namespace UnitTests.FakeLoaderClasses
{
    internal class FakeContentMetaDataLoader : ContentMetaDataLoader<TestActivitySummary>
    {
        private readonly int _reportsSummaryCountWanted;

        public FakeContentMetaDataLoader(ILogger debugTracer, int reportsCountWanted) : base(debugTracer)
        {
            _reportsSummaryCountWanted = reportsCountWanted;
        }

        protected override Task<List<TestActivitySummary>> LoadAllActivityReports(string auditContentType, TimePeriod chunk, int batchId)
        {
            var list = new List<TestActivitySummary>();
            for (int i = 0; i < _reportsSummaryCountWanted; i++)
            {
                list.Add(new TestActivitySummary { Created = DateTime.Now.AddSeconds(i) });
            }
            return Task.FromResult(list);
        }
    }
}
