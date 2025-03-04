using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.O365UsageReports.Models;

/// <summary>
/// https://docs.microsoft.com/en-us/graph/api/reportroot-getyammeractivityuserdetail?view=graph-rest-beta
/// </summary>
public class YammerUserActivityUserDetail : AbstractUserActivityUserRecordWithUpn
{
    [JsonProperty("postedCount")]
    public int? PostedCount { get; set; }

    [JsonProperty("readCount")]
    public int? ReadCount { get; set; }

    [JsonProperty("likedCount")]
    public int? LikedCount { get; set; }
}


/// <summary>
/// https://docs.microsoft.com/en-us/graph/api/reportroot-getyammerdeviceusageuserdetail?view=graph-rest-beta
/// </summary>
public class YammerDeviceActivityDetail : AbstractUserActivityUserRecordWithUpn
{

    [JsonProperty("usedWeb")]
    public bool? UsedWeb { get; set; }

    [JsonProperty("usedWindowsPhone")]
    public bool? UsedWindowsPhone { get; set; }

    [JsonProperty("usedAndroidPhone")]
    public bool? UsedAndroidPhone { get; set; }

    [JsonProperty("usediPad")]
    public bool? UsedIpad { get; set; }

    [JsonProperty("usediPhone")]
    public bool? UsedIphone { get; set; }

    [JsonProperty("usedOthers")]
    public bool? UsedOthers { get; set; }
}
