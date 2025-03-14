

using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class OfficeLocationCache : DBLookupCache<UserOfficeLocation>
{
    public OfficeLocationCache(DataContext context) : base(context) { }

    public override DbSet<UserOfficeLocation> EntityStore => DB.UserOfficeLocations;

    public async override Task<UserOfficeLocation?> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}
