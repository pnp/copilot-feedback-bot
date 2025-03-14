using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

/// <summary>
/// Implementation for classes that inherit from AbstractEFEntityWithName
/// </summary>
public abstract class EntityWithNameDBLookupCache<T> : DBLookupCache<T> where T : AbstractEFEntityWithName, new()
{
    protected EntityWithNameDBLookupCache(DataContext context) : base(context)
    {
    }

    public async override Task<T?> Load(string searchKey)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchKey);
    }

    public Task<T> GetOrCreateNewResource(string key)
    {
        return base.GetOrCreateNewResource(key, new T { Name = key });
    }
}
