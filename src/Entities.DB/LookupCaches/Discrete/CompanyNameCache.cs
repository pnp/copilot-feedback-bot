

using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class CompanyNameCache : DBLookupCache<CompanyName>
{
    public CompanyNameCache(DataContext context) : base(context) { }

    public override DbSet<CompanyName> EntityStore => DB.CompanyNames;

    public async override Task<CompanyName?> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}
