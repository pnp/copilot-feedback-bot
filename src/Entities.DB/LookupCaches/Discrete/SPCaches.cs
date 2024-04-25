using Entities.DB.Entities.AuditLog;
using Entities.DB.Entities.SP;
using Microsoft.EntityFrameworkCore;

namespace Entities.DB.LookupCaches.Discrete;

public class SPFileNameCache : EntityWithNameDBLookupCache<SPEventFileName>
{
    public SPFileNameCache(DataContext context) : base(context) { }

    public override DbSet<SPEventFileName> EntityStore => DB.SharePointFileNames;
}

public class SPFileExtensionCache : EntityWithNameDBLookupCache<SPEventFileExtension>
{
    public SPFileExtensionCache(DataContext context) : base(context) { }

    public override DbSet<SPEventFileExtension> EntityStore => DB.SharePointFileExtensions;
}

public class UrlCache : DBLookupCache<Url>
{
    public UrlCache(DataContext context) : base(context) { }

    public override DbSet<Url> EntityStore => DB.Urls;

    public async override Task<Url?> Load(string searchKey)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.FullUrl == searchKey);
    }
}

public class SiteCache : DBLookupCache<Site>
{
    public SiteCache(DataContext context) : base(context) { }

    public override DbSet<Site> EntityStore => DB.Sites;

    public async override Task<Site?> Load(string url)
    {
        return await EntityStore.SingleOrDefaultAsync(t => t.UrlBase == url);
    }
}