using Entities.DB.Entities.SP;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.AuditLog;

[Table("event_meta_sharepoint")]
public class SharePointEventMetadata : BaseOfficeEvent
{
    [ForeignKey(nameof(FileExtension))]
    [Column("file_extension_id")]
    public int? FileExtensionId { get; set; } = 0;
    public SPEventFileExtension? FileExtension { get; set; } = null!;

    [ForeignKey(nameof(FileName))]
    [Column("file_name_id")]
    public int? FileNameId { get; set; } = 0;
    public SPEventFileName? FileName { get; set; } = null!;


    [ForeignKey(nameof(Url))]
    [Column("url_id")]
    public int UrlId { get; set; } = 0;
    public Url Url { get; set; } = null!;


    [ForeignKey(nameof(Site))]
    [Column("related_site_id")]
    public int? RelatedSiteId { get; set; } = 0;
    public Site? Site { get; set; } = null;

    [ForeignKey(nameof(ItemType))]
    [Column("item_type_id")]
    public int ItemTypeId { get; set; } = 0;
    public SPEventType ItemType { get; set; } = null!;

}
