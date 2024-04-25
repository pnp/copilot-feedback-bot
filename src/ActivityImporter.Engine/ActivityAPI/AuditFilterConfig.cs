using ActivityImporter.Engine.ActivityAPI.Models;
using Common.DataUtils;
using Entities.DB;
using Microsoft.Extensions.Logging;

namespace ActivityImporter.Engine.ActivityAPI;

/// <summary>
/// Represents logic for filtering if an audit log event should be included in the import
/// </summary>
public abstract class AuditFilterConfig
{
    public virtual bool InScope(AbstractAuditLogContent content) => true;
}

public class SharePointOrgUrlsFilterConfig : AuditFilterConfig
{
    public override bool InScope(AbstractAuditLogContent content)
    {
        // Only include SharePoint events that are in the URLs we're interested in
        string url = content.ObjectId;

        // Do we have a URL?
        if (!string.IsNullOrEmpty(url))
        {
            var siteFilter = string.Empty;
            if (content is SharePointAuditLogContent)
            {
                // Find an org URL that exactly matches the sharepoint event
                var spContent = (SharePointAuditLogContent)content;
                siteFilter = spContent.SiteUrl;
            }

            // Analyse all org URLs to see which one matches this hit
            if (siteFilter != null)
            {
                return OrgUrlConfigs.UrlInScope(siteFilter, url);
            }
            else
            {
                return false;       // No site URL to match against, assume out of scope
            }
        }

        // Something happened in SharePoint/OneDrive without a URL ("ManagedSyncClientAllowed" for example). Assume we want it
        return true;
    }

    public List<FilterUrlConfig> OrgUrlConfigs { get; set; } = new List<FilterUrlConfig>();
    public static async Task<SharePointOrgUrlsFilterConfig> Load(DataContext db)
    {
        var orgUrlConfigs = await SiteFilterLoader.Load(db);

        return new SharePointOrgUrlsFilterConfig
        {
            OrgUrlConfigs = orgUrlConfigs
        };
    }

    public void Print(ILogger telemetry)
    {
        foreach (var url in OrgUrlConfigs)
        {
            if (url.ExactSiteMatch)
            {
                telemetry.LogInformation($"+{url.Url} (exact match)");
            }
            else
            {
                telemetry.LogInformation($"+{url.Url} (*)");
            }
        }
    }
}
