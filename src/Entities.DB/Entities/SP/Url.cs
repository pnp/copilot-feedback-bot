using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.SP;


/// <summary>
/// A URL of a hit/file in SPO
/// </summary>
[Table("urls")]
public class Url : AbstractEFEntity
{

    [Column("full_url")]
    [MaxLength(2048)]
    public string FullUrl { get; set; } = null!;

    [Column("file_last_refreshed")]
    public DateTime? MetadataLastRefreshed { get; set; } = null;

    public override string ToString()
    {
        return $"{base.ToString()},{nameof(FullUrl)}={FullUrl}";
    }
}
