using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.SP;

/// <summary>
/// URL used for importing activity for from O365. 
/// </summary>
[Table("import_url_filter")]
public class ImportSiteFilter : AbstractEFEntity
{
    [Column("url_base")]
    public string UrlBase { get; set; } = null!;

    [Column("exact_match")]
    public bool? ExactMatch { get; set; } = false;
}
