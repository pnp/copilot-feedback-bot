using Entities.DB.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB;

// Make sure we keep certain naming conventions and properties in sync
public interface IUserRelatedEntity
{
    [ForeignKey(nameof(User))]
    [Column("user_id")]
    int UserID { get; set; }

    User User { get; set; }
}