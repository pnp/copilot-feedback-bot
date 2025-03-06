using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

/// <summary>
/// Type of object the event occured on (file, page, web, folder, etc). Only used for SharePoint events. 
/// </summary>
/// 
[Table("event_types")]
public class SPEventType : AbstractEFEntityWithName
{
}
