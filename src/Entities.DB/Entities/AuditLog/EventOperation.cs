using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

/// <summary>
/// The operation done that generated the audit-event. FilePreviewed, FileCheckedOut, New-Mailbox, etc
/// </summary>
/// 
[Table("event_operations")]
public class EventOperation : AbstractEFEntityWithName
{
}
