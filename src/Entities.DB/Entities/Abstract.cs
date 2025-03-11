using Entities.DB.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities;
/// <summary>
/// Base implementation for all EF classes. Not fully rolled out to all classes yet.
/// </summary>
public abstract class AbstractEFEntity
{
    [Key]
    [Column("id")]
    public int ID { get; set; }

    public bool IsSavedToDB
    {
        get { return ID != 0; }
    }


    public override string ToString()
    {
        return $"{GetType().Name}: {nameof(ID)}={ID}";
    }
}

/// <summary>
/// Base implementation for all EF classes with name field
/// </summary>
public abstract class AbstractEFEntityWithName : AbstractEFEntity, ILookupEntity
{
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"{base.ToString()},{nameof(Name)}={Name}";
    }
}

public abstract class UserRelatedEntity : AbstractEFEntity
{
    public User User { get; set; } = null!;

    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public int UserID { get; set; }
}
