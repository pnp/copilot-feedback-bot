using Entities.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.Profiling;



/// <summary>
/// Activities in a service in O365 ('Teams Post Messages'), per activity + user, per week
/// </summary>
public class ActivitiesWeekly : IActivitiesWeeklyRecord
{
    public string Metric { get; set; } = string.Empty;
    public int Sum { get; set; } = 0;

    public DateOnly MetricDate { get; set; }

    public User User { get; set; } = null!;

    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public int UserID { get; set; }

    ITrackedUser IActivitiesWeeklyRecord.User { get => User; }
}

/// <summary>
/// ActivitiesWeekly, but each activity is a column
/// </summary>
public class ActivitiesWeeklyColumns
{
}

/// <summary>
/// Per user, per week, each app they used
/// </summary>
public class UsageWeekly
{
}
