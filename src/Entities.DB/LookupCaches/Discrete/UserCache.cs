using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class UserCache : DBLookupCache<User>
{
    public UserCache(DataContext context) : base(context) { }

    public override DbSet<User> EntityStore => DB.Users;

    public async override Task<User?> Load(string upn)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.UserPrincipalName == upn);
    }
}
