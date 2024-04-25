using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog
{

    /// <summary>
    /// Office 365 audit events that have been processed already. 
    /// </summary>
    [Table("ignored_audit_events")]
    public class IgnoredEvent
    {
        [Key]
        public Guid event_id { get; set; }
        public DateTime processed_timestamp { get; set; }
    }
}
