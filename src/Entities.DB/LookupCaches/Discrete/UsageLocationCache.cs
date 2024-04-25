

using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete
{
    public class UsageLocationCache : DBLookupCache<UserUsageLocation>
    {
        public UsageLocationCache(DataContext context) : base(context) { }

        public override DbSet<UserUsageLocation> EntityStore => DB.UserUsageLocations;

        public async override Task<UserUsageLocation?> Load(string searchName)
        {
            return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
        }
    }
}
