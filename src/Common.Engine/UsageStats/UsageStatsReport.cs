using Entities.DB.Models;
using System.Linq;

namespace Common.Engine.UsageStats;

public class UsageStatsReport
{
    public UsageStatsReport(IEnumerable<IActivitiesWeeklyRecord> fromData) : this(fromData, new UsageStatsReportFilter())
    {
    }
    public UsageStatsReport(IEnumerable<IActivitiesWeeklyRecord> fromData, UsageStatsReportFilter filter)
    {

        var filteredUsers = fromData
            .Select(x => x.User)
            .Distinct()
            .ByFilter(filter)
            .ToList();

        var filteredData = fromData
            .Where(x => filteredUsers.Contains(x.User))
            .ToList();


        this.UniqueActivities = filteredData
            .Select(x => x.Metric)
            .Distinct()
            .ToList();


        this.Dates = filteredData
            .Select(x => x.MetricDate)
            .Distinct()
            .ToList();

        foreach (var user in filteredUsers)
        {
            var score = filteredData
                .Where(x => x.User == user)
                .Sum(x => x.Sum);
            this.Users.Add(new UserWithScore(user, score));
        }
    }

    public List<string> UniqueActivities { get; set; } = new();

    public List<DateOnly> Dates { get; set; } = new();

    public List<UserWithScore> Users { get; set; } = new();
}

public class UserWithScore
{
    public UserWithScore(ITrackedUser user, int score)
    {
        User = user;
        Score = score;
    }
    public ITrackedUser User { get; set; }
    public int Score { get; set; }
}
