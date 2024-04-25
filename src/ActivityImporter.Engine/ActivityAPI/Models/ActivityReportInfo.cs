using Newtonsoft.Json;

namespace ActivityImporter.Engine.ActivityAPI.Models;


public abstract class BaseActivityReportInfo
{
    [JsonProperty("contentCreated")]
    public DateTime Created { get; set; }
}

/// <summary>
/// Data class to deserialise the metadata into. 
/// https://msdn.microsoft.com/en-us/office-365/office-365-management-activity-api-reference
/// </summary>
public class ActivityReportInfo : BaseActivityReportInfo
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; } = null!;

    [JsonProperty("contentId")]
    public string ContentId { get; set; } = null!;

    [JsonProperty("contentUri")]
    public Uri ContentUri { get; set; } = null!;


    /// <summary>
    /// The batch number this activity report was found on. Generated for a specific time-chunk.
    /// </summary>
    public int BatchID { get; set; }

    public override string ToString()
    {
        return $"{ContentType}: {Created}, ID:{ContentId}";
    }
}
