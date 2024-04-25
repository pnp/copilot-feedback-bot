using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.SP;

[Table("sites")]
public class Site : AbstractEFEntity
{
    [MaxLength(500)]
    [Column("url_base")]
    public string UrlBase { get; set; } = null!;
}
