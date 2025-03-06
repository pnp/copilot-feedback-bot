using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;


/// <summary>
/// Office 365 audit events that have been processed already. 
/// </summary>
[Table("ignored_audit_events")]
public class IgnoredEvent
{
    [Key]
    [Column("event_id")]
    public Guid AuditEventId { get; set; }

    [Column("processed_timestamp")]
    public DateTime Processed { get; set; }
}
