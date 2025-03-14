using Common.DataUtils;
using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete
{
    /// <summary>
    /// Base cache implementation
    /// </summary>
    /// <typeparam name="T">Type of entity being cached</typeparam>
    public abstract class DBLookupCache<T> : ObjectByIdCache<T> where T : AbstractEFEntity
    {
        protected DataContext DB { get; set; }
        public DBLookupCache(DataContext context)
        {
            DB = context;
        }

        /// <summary>
        /// Object not found in DB. Adding to database.
        /// </summary>
        public event EventHandler<T>? NewObjectCreating;

        /// <summary>
        /// Loads from cache or if doesn't exist in cache, from DB & adds to cache for next time.
        /// Doesn't save on insert by default.
        /// </summary>
        public async virtual Task<T> GetOrCreateNewResource(string key, T newTemplate)
        {
            return await GetOrCreateNewResource(key, newTemplate, false);
        }
        /// <summary>
        /// Loads from cache or if doesn't exist in cache, from DB & adds to cache for next time.
        /// </summary>
        public async virtual Task<T> GetOrCreateNewResource(string key, T newTemplate, bool commitChangeOnSaveNew)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Trim as SQL will also do so - https://support.microsoft.com/en-gb/topic/inf-how-sql-server-compares-strings-with-trailing-spaces-b62b1a2d-27d3-4260-216d-a605719003b0
            key = key.Trim();

            return await GetResource(key, async () =>
            {
                NewObjectCreating?.Invoke(this, newTemplate);

                EntityStore.Add(newTemplate);
                if (commitChangeOnSaveNew)
                {
                    await DB.SaveChangesAsync();
                }
                return newTemplate;
            });
        }

        public abstract DbSet<T> EntityStore { get; }

    }
}
