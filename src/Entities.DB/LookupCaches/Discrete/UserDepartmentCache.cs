
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete
{

    // User caches
    public class UserDepartmentCache : DBLookupCache<UserDepartment>
    {
        public UserDepartmentCache(DataContext context) : base(context) { }

        public override DbSet<UserDepartment> EntityStore => DB.UserDepartments;

        public async override Task<UserDepartment?> Load(string searchName)
        {
            return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
        }
    }
}
