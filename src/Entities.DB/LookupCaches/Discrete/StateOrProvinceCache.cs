using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;


public class StateOrProvinceCache : DBLookupCache<StateOrProvince>
{
    public StateOrProvinceCache(DataContext context) : base(context) { }

    public override DbSet<StateOrProvince> EntityStore => this.DB.StateOrProvinces;

    public async override Task<StateOrProvince> Load(string searchName)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.Name == searchName);
    }
}