using Common.DataUtils;
using Entities.DB;
using Entities.DB.Entities.SP;
using Microsoft.EntityFrameworkCore;

namespace ActivityImporter.Engine;

public class SiteFilterLoader
{
    /// <summary>
    /// Load from the DB our SharePoint filter config
    /// </summary>
    public static async Task<List<FilterUrlConfig>> Load(DataContext db)
    {
        var allOrgURLs = from urls in db.ImportSiteFilters
                         select urls;

        var orgUrlCache = await allOrgURLs.ToListAsync();
#if DEBUG
        if (orgUrlCache.Count == 0)
        {
            db.ImportSiteFilters.Add(new ImportSiteFilter() { UrlBase = "https://" });
            db.ImportSiteFilters.Add(new ImportSiteFilter() { UrlBase = "https://DEVBOXSHAREPOINT" });
            await db.SaveChangesAsync();
            orgUrlCache = db.ImportSiteFilters.ToList();
        }
#endif


        return orgUrlCache.Select(u => new FilterUrlConfig
        {
            Url = u.UrlBase,
            ExactSiteMatch = u.ExactMatch.HasValue && u.ExactMatch.Value
        }).ToList();
    }
}
