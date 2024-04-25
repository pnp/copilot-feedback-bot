using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

/// <summary>
/// The common event entity for any workload. Workload specific events link back to this.
/// </summary>
[Table("audit_events")]
public class CommonAuditEvent
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("time_stamp")]
    public DateTime TimeStamp { get; set; }

    [ForeignKey(nameof(Operation))]
    [Column("operation_id")]
    public int OperationId { get; set; } = 0;
    public EventOperation Operation { get; set; } = null!;

    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public int UserId { get; set; } = 0;
    public User User { get; set; } = null!;
}
