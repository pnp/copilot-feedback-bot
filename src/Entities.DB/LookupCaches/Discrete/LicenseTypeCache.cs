using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class LicenseTypeCache : DBLookupCache<LicenseType>
{
    public LicenseTypeCache(DataContext context) : base(context) { }

    public override DbSet<LicenseType> EntityStore => this.DB.LicenseTypes;

    public async override Task<LicenseType> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}