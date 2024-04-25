using Entities.DB.Entities;
using Entities.DB.LookupCaches.Discrete;
using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.O365UsageReports.Models;

/// <summary>
/// A Graph activity record with a lookup to another record (usually "user")
/// </summary>
public abstract class AbstractActivityRecord
{
    [JsonProperty("lastActivityDate")]
    public string LastActivityDateString { get; set; } = null!;

    /// <summary>
    /// Get associated DB lookup record associated with this activity. Usually a user.
    /// Must save to the DB if creating new so we get a PK ID.
    /// </summary>
    public abstract Task<AbstractEFEntity> GetOrCreateLookup(UserCache userCache);

    /// <summary>
    /// Field-value that points to the activity record lookup name
    /// </summary>
    public abstract string LookupFieldValue { get; }
}

public abstract class AbstractUserActivityUserRecord : AbstractActivityRecord
{

    /// <summary>
    /// The field-value in a user activity report that identifies the related user
    /// </summary>
    public abstract string UPNFieldVal { get; }

    public override string LookupFieldValue => UPNFieldVal;

    public override async Task<AbstractEFEntity> GetOrCreateLookup(UserCache userCache)
    {
        return await userCache.GetOrCreateNewResource(UPNFieldVal, new User { UserPrincipalName = UPNFieldVal }, true);
    }
}

public abstract class AbstractUserActivityUserRecordWithUpn : AbstractUserActivityUserRecord
{
    public override string UPNFieldVal => UserPrincipalName;

    [JsonProperty("userPrincipalName")]
    public string UserPrincipalName { get; set; } = null!;
}
