using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

public abstract class BaseOfficeEvent
{
    /// <summary>
    /// Foriegn key for "Event" only
    /// </summary>
    [Key]
    [ForeignKey(nameof(AuditEvent))]
    [Column("event_id")]
    public Guid AuditEventID { get; set; }

    public CommonAuditEvent AuditEvent { get; set; } = null!;
}
