using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.O365UsageReports.Models;


/// <summary>
/// https://docs.microsoft.com/en-us/graph/api/reportroot-getteamsdeviceusageuserdetail?view=graph-rest-beta
/// </summary>
public class TeamsDeviceUsageUserDetail : AbstractUserActivityUserRecordWithUpn
{
    [JsonProperty("reportRefreshDate")]
    public string ReportRefreshDate { get; set; }


    [JsonProperty("isLicensed")]
    public bool? IsLicensed { get; set; }

    [JsonProperty("lastActivityDate")]
    public string LastActivityDate { get; set; }

    [JsonProperty("isDeleted")]
    public bool? IsDeleted { get; set; }

    [JsonProperty("deletedDate")]
    public string DeletedDate { get; set; }

    [JsonProperty("usedWeb")]
    public bool? UsedWeb { get; set; }

    [JsonProperty("usedWindowsPhone")]
    public bool? UsedWindowsPhone { get; set; }

    [JsonProperty("usediOS")]
    public bool? UsediOS { get; set; }

    [JsonProperty("usedMac")]
    public bool? UsedMac { get; set; }

    [JsonProperty("usedAndroidPhone")]
    public bool? UsedAndroidPhone { get; set; }

    [JsonProperty("usedWindows")]
    public bool? UsedWindows { get; set; }

    [JsonProperty("usedChromeOS")]
    public bool? UsedChromeOs { get; set; }

    [JsonProperty("usedLinux")]
    public bool? UsedLinux { get; set; }

    [JsonProperty("reportPeriod")]
    public string ReportPeriod { get; set; }
}