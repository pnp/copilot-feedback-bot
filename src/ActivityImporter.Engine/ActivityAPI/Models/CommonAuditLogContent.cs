using Entities.DB.Entities.AuditLog;

namespace ActivityImporter.Engine.ActivityAPI.Models;

/// <summary>
/// JSon entity for audit-log response. Deserialised in AuditLogContentSet.LoadFromWeb
/// Reference: https://docs.microsoft.com/en-us/office/office-365-management-api/office-365-management-activity-api-schema
/// </summary>
public abstract class AbstractAuditLogContent : WorkloadOnlyAuditLogContent, IEquatable<AbstractAuditLogContent>
{
    /// <summary>
    /// JSon used to deserialise the content-set. Used in import-log, and generic events for all data. 
    /// </summary>
    public string OriginalImportFileContents { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
    public Guid Id { get; set; }

    public string Operation { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string ItemType { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;

    /// <summary>
    /// Save new common + specific event to SQL.
    /// </summary>
    public abstract Task<bool> ProcessExtendedProperties(ActivityLogSaveSession saveBatch, CommonAuditEvent relatedAuditEvent);

    #region IEquatable<AuditLogContent>

    public bool Equals(AbstractAuditLogContent? other)
    {
        if (other == null)
        {
            return false;
        }

        // Hack?
        return Id == other.Id && other.OriginalImportFileContents == OriginalImportFileContents;
    }

    #endregion
}

public enum SaveResultEnum
{
    NotSaved = 0,           // Default
    ProcessedAlready = 1,   // Event ignored previously
    Imported = 2,           // Already imported
    OutOfScope = 3          // Not to be imported. Usually because the SharePoint URL is for a site we don't care about.
}
