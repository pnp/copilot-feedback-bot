using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.UsageReports;

[Table("onedrive_user_activity_log")]
public class OneDriveUserActivityLog : UserRelatedAbstractUsageActivity
{
    [Column("viewed_or_edited")]
    public long ViewedOrEdited { get; set; }

    [Column("synced")]
    public long Synced { get; set; }

    [Column("shared_internally")]
    public long SharedInternally { get; set; }

    [Column("shared_externally")]
    public long SharedExternally { get; set; }

}
