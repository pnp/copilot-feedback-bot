using ActivityImporter.Engine.Graph.O365UsageReports;
using ActivityImporter.Engine.Graph.O365UsageReports.ReportLoaders;
using Common.Engine.UsageStats;
using Entities.DB;
using Entities.DB.Entities;
using Entities.DB.Entities.Profiling;
using Entities.DB.Models;
using Microsoft.EntityFrameworkCore;
using UnitTests.FakeLoaderClasses;

namespace UnitTests;

[TestClass]
public class UsageTests : AbstractTest
{
    private TestUser user1 = new TestUser
    {
        UserPrincipalName = "user1@contoso.local",
        CompanyName = "Contoso",
        StateOrProvince = "WA",
        UserCountry = "US",
        OfficeLocation = "Seattle",
        UsageLocation = "US",
        Department = "IT",
        JobTitle = "Developer",
    };
    private TestUser user2 = new TestUser
    {
        UserPrincipalName = "user2",
        CompanyName = "Contoso",
        StateOrProvince = "WA",
        UserCountry = "US",
        OfficeLocation = "Seattle",
        UsageLocation = "US",
        Department = "Engineering",
        JobTitle = "Developer",
    };

    [TestMethod]
    public async Task ReportManagerTests()
    {
        var filter = new LoaderUsageStatsReportFilter
        {
            From = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            To = DateOnly.FromDateTime(DateTime.UtcNow)
        };


        var optionsBuilder = new DbContextOptionsBuilder<ProfilingContext>();
        optionsBuilder.UseSqlServer(_config.ConnectionStrings.SQL);

        var db = new ProfilingContext(optionsBuilder.Options);

        var testUSName = "US-" + DateTime.Now.Ticks;
        db.ActivitiesWeekly.RemoveRange(db.ActivitiesWeekly);
        db.ActivitiesWeekly.Add(new ActivitiesWeekly
        {
            Metric = "Teams Meetings Attended",
            MetricDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            Sum = 1,
            User = new User 
            { 
                UserPrincipalName = "user1-" + DateTime.Now.Ticks,
                UserCountry = new CountryOrRegion { Name = testUSName }
            }
        });
        db.ActivitiesWeekly.Add(new ActivitiesWeekly
        {
            Metric = "Teams Meetings Attended",
            MetricDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Sum = 1,
            User = new User { UserPrincipalName = "user2-" + DateTime.Now.Ticks }
        });
        await db.SaveChangesAsync();

        var reportManager = new ReportManager(new SqlUsageDataLoader(db), GetLogger<ReportManager>());
        var report = await reportManager.GetReport(filter);
        Assert.IsNotNull(report);

        Assert.AreEqual(2, report.Users.Count);

        filter.From = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var reportWithNewDateFilter = await reportManager.GetReport(filter);
        Assert.IsNotNull(reportWithNewDateFilter);
        Assert.AreEqual(1, reportWithNewDateFilter.Users.Count);
    }

    [TestMethod]
    public void UsageStatsReportTests()
    {
        var testData = new List<TestActivitiesWeeklyRecord>
        {
            new TestActivitiesWeeklyRecord
            {
                Metric = "Teams Meetings Attended",
                MetricDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Sum = 1,
                User = user1
            },
            new TestActivitiesWeeklyRecord
            {
                Metric = "Teams Team Chats",
                MetricDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                Sum = 2,
                User = user2
            },
        };

        var reportAllData = new UsageStatsReport(testData.Cast<IActivitiesWeeklyRecord>());

        Assert.AreEqual(2, reportAllData.Users.Count);
        Assert.AreEqual(2, reportAllData.UniqueActivities.Count);
        Assert.AreEqual(2, reportAllData.Dates.Count);
        Assert.AreEqual("Teams Meetings Attended", reportAllData.UniqueActivities[0]);
        Assert.AreEqual("Teams Team Chats", reportAllData.UniqueActivities[1]);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), reportAllData.Dates[0]);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), reportAllData.Dates[1]);
        Assert.AreEqual(1, reportAllData.Users[0].Score);
        Assert.AreEqual(2, reportAllData.Users[1].Score);

        var reportFilteredData = new UsageStatsReport(testData.Cast<IActivitiesWeeklyRecord>(), new UsageStatsReportFilter { InDepartments = new List<string> { "IT" } });
        Assert.AreEqual(1, reportFilteredData.Users.Count);
        Assert.AreEqual(1, reportFilteredData.UniqueActivities.Count);
        Assert.AreEqual(1, reportFilteredData.Dates.Count);
        Assert.AreEqual("Teams Meetings Attended", reportFilteredData.UniqueActivities[0]);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), reportFilteredData.Dates[0]);
        Assert.AreEqual(1, reportFilteredData.Users[0].Score);
        Assert.AreEqual(user1.UserPrincipalName, reportFilteredData.Users[0].User.UserPrincipalName);
    }

    [TestMethod]
    public void UserFilterTests()
    {
        var users = new List<TestUser> { user1, user2 };

        var resultsByDepartmentsAndCountries = users.ByFilter(new UsageStatsReportFilter { InDepartments = new List<string> { "IT" }, InCountries = new List<string> { "US" } });
        Assert.AreEqual(1, resultsByDepartmentsAndCountries.Count());

        var resultsNoFilter = users.ByFilter(new UsageStatsReportFilter { InDepartments = new List<string>() });
        Assert.AreEqual(2, resultsNoFilter.Count());
    }

    [TestMethod]
    public async Task FakeTeamsActivity()
    {
        var activityLoader = new FakeUserActivityLoader();
        var loader = new TeamsUserDeviceLoader(activityLoader, new SqlUsageReportPersistence(_db, _logger), _logger);
        var p = await loader.LoadAndSaveUsagePages();
        Assert.AreEqual(TeamsUserDeviceLoader.MAX_DAYS_BACK - 1, p.Count);
    }
}

class TestActivitiesWeeklyRecord : IActivitiesWeeklyRecord
{
    public string Metric { get; set; } = string.Empty;
    public DateOnly MetricDate { get; set; }
    public int Sum { get; set; }
    public TestUser User { get; set; } = null!;

    ITrackedUser IActivitiesWeeklyRecord.User => User;
}

class TestUser : ITrackedUser
{
    public string UserPrincipalName { get; set; } = null!;

    public string? CompanyName { get; set; } = null!;

    public string? StateOrProvince { get; set; } = null!;

    public User? Manager { get; set; } = null!;

    public string? UserCountry { get; set; } = null!;

    public string? OfficeLocation { get; set; } = null!;

    public string? UsageLocation { get; set; } = null!;

    public string? Department { get; set; } = null!;

    public string? JobTitle { get; set; } = null!;

    public List<string> Licenses { get; set; } = new List<string>();

    ITrackedUser? ITrackedUser.Manager => Manager;

}
