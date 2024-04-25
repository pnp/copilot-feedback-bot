using ActivityImporter.Engine.ActivityAPI;
using ActivityImporter.Engine.ActivityAPI.Models;
using Entities.DB.Entities;
using UnitTests.ActivityImporter;

namespace UnitTests.FakeLoaderClasses;

internal class FakeActivityReportLoader : IActivityReportLoader<TestActivitySummary>
{
    private int _reportsCount;
    private readonly User _forUser;

    public FakeActivityReportLoader(int reportsCount, User forUser)
    {
        _reportsCount = reportsCount;
        _forUser = forUser;
    }

    public Task<WebActivityReportSet> Load(ActivityReportInfo metadata)
    {
        return Task.FromResult(new WebActivityReportSet(metadata));
    }

    public Task<ActivityReportSet> Load(TestActivitySummary metadata)
    {
        var reports = new WebActivityReportSet();
        for (int i = 0; i < _reportsCount; i++)
        {
            var r = DataGenerators.GetRandomSharePointLog();
            r.UserId = _forUser.UserPrincipalName;
            reports.Add(r);
        }

        // Add an invalid one too
        var rInvalid = DataGenerators.GetRandomSharePointLog();
        rInvalid.UserId = _forUser.UserPrincipalName;
        rInvalid.SiteUrl = null!;
        reports.Add(rInvalid);

        return Task.FromResult((ActivityReportSet)reports);
    }
}
