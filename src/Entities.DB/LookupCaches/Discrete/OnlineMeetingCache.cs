using Entities.DB.DbContexts;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class OnlineMeetingCache : EntityWithNameDBLookupCache<OnlineMeeting>
{
    public OnlineMeetingCache(DataContext context) : base(context)
    {
    }

    public override DbSet<OnlineMeeting> EntityStore => DB.OnlineMeetings;
}
