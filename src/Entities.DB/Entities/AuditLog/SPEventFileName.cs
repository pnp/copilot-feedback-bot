using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;


/// <summary>
/// The file name involved in an audit event. Only used in SharePoint events. 
/// </summary>
/// 
[Table("event_file_names")]
public class SPEventFileName : AbstractEFEntityWithName
{
}
