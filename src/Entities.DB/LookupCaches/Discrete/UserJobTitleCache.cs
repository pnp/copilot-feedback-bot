

using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class UserJobTitleCache : DBLookupCache<UserJobTitle>
{
    public UserJobTitleCache(DataContext context) : base(context) { }

    public override DbSet<UserJobTitle> EntityStore => DB.UserJobTitles;

    public async override Task<UserJobTitle?> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}
