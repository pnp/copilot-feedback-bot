using ActivityImporter.Engine.ActivityAPI;
using ActivityImporter.Engine.ActivityAPI.Loaders;
using ActivityImporter.Engine.ActivityAPI.Models;
using Entities.DB.Entities;
using Microsoft.Extensions.Logging;

namespace UnitTests.FakeLoaderClasses;

internal class FakeActivityImporter : ActivityImporter<TestActivitySummary>
{
    public const int MAX_REPORTS_PER_SUMMARY = 100;
    private FakeActivityReportLoader _reportLoader;
    private FakeContentMetaDataLoader _contentMetaDataLoader;
    private FakeActivitySubscriptionManager _activitySubscriptionManager;
    public FakeActivityImporter(int reportsWanted, User forUser, ILogger telemetry) : base(telemetry, 1)
    {
        _reportLoader = new FakeActivityReportLoader(1, forUser);
        _contentMetaDataLoader = new FakeContentMetaDataLoader(telemetry, reportsWanted);
        _activitySubscriptionManager = new FakeActivitySubscriptionManager();
    }

    public override IActivityReportLoader<TestActivitySummary> ReportLoader => _reportLoader;

    public override ContentMetaDataLoader<TestActivitySummary> ContentMetaDataLoader => _contentMetaDataLoader;

    public override IActivitySubscriptionManager ActivitySubscriptionManager => _activitySubscriptionManager;
}

public class TestActivitySummary : BaseActivityReportInfo
{

}
