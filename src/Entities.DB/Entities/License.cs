using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DB.Entities;


[Table("user_license_type_lookups")]
public class UserLicenseTypeLookup : AbstractEFEntity
{
    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public int UserId { get; set; }

    public User User { get; set; }

    [ForeignKey(nameof(License))]
    [Column("license_type_id")]
    public int LicenseTypeId { get; set; }

    public LicenseType License { get; set; }

}

[Table("license_types")]
public class LicenseType : AbstractEFEntityWithName
{
    [Column("sku_id")]
    public string SKUID { get; set; }
}
