using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities;

[Table("online_meetings")]
public class OnlineMeeting : AbstractEFEntityWithName
{
    [Column("created")]
    public DateTime CreatedUTC { get; set; }
    [Column("meeting_id")]
    public string MeetingId { get; set; } = null!;
}
