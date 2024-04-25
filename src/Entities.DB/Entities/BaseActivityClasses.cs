using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities;

/// <summary>
/// One of the activity reports in Graph
/// </summary>
public abstract class AbstractUsageActivityLog : UserRelatedEntity
{
    [Column("date")]
    public DateTime Date { get; set; }

    [Column("last_activity_date")]
    public DateTime? LastActivityDate { get; set; }

    [NotMapped]
    public abstract int AssociatedLookupId { get; set; }

    public override string ToString()
    {
        return $"{GetType().Name} - {Date}";
    }
}

/// <summary>
/// One of the activity reports in Graph
/// </summary>
public class UserRelatedAbstractUsageActivity : AbstractUsageActivityLog, IUserRelatedEntity
{

    public override int AssociatedLookupId { get => UserID; set => UserID = value; }
}
