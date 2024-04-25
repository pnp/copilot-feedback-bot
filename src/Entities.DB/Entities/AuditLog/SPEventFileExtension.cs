using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

/// <summary>
/// File extension for an O365 audit event.
/// </summary>
/// 
[Table("event_file_ext")]
public class SPEventFileExtension : AbstractEFEntityWithName
{
}
