using Entities.DB.Entities.AuditLog;

namespace ActivityImporter.Engine.ActivityAPI.Models;

public class SharePointAuditLogContent : AbstractAuditLogContent
{
    public string? SiteUrl { get; set; } = null;

    public string SourceFileName { get; set; } = null!;

    public string SourceFileExtension { get; set; } = null!;

    public override Task<bool> ProcessExtendedProperties(ActivityLogSaveSession saveBatch, CommonAuditEvent relatedAuditEvent)
    {
        return Task.FromResult(false);
    }
}
