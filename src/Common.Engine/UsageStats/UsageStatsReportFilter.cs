using Entities.DB.Models;

namespace Common.Engine.UsageStats;

public class UsageStatsReportFilter
{
    public List<string> InDepartments { get; set; } = new();

    public List<string> InCountries { get; set; } = new();
}


public static class UsageStatsReportFilterFunc
{
    public static IEnumerable<ITrackedUser> ByFilter(this IEnumerable<ITrackedUser> source, UsageStatsReportFilter filter)
    {
        return source
            .ByDepartments(filter.InDepartments)
            .ByCountries(filter.InCountries);
    }

    static IEnumerable<ITrackedUser> ByDepartments(this IEnumerable<ITrackedUser> source, List<string> inList)
    {
        return source
            .ByProperty(inList, user => user.Department);
    }
    static IEnumerable<ITrackedUser> ByCountries(this IEnumerable<ITrackedUser> source, List<string> inList)
    {
        return source
            .ByProperty(inList, user => user.UserCountry);
    }

    static IEnumerable<ITrackedUser> ByProperty(this IEnumerable<ITrackedUser> source, List<string> inList, Func<ITrackedUser, string?> propertySelector)
    {
        return source
            .Where(user => inList.Count == 0 ||
                (inList.Count > 0 && propertySelector(user) != null && inList.Contains(propertySelector(user)!)));
    }

}
