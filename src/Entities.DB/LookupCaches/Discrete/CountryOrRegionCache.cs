

using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class CountryOrRegionCache : DBLookupCache<CountryOrRegion>
{
    public CountryOrRegionCache(DataContext context) : base(context) { }

    public override DbSet<CountryOrRegion> EntityStore => DB.CountryOrRegions;

    public async override Task<CountryOrRegion?> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}
