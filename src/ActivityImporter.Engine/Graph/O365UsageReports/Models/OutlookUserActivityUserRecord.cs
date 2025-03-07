using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.O365UsageReports.Models;

public class OutlookUserActivityUserRecord : AbstractUserActivityUserRecordWithUpn
{

    [JsonProperty("sendCount")]
    public int SendCount { get; set; }


    [JsonProperty("receiveCount")]
    public int ReceiveCount { get; set; }


    [JsonProperty("readCount")]
    public int ReadCount { get; set; }

    [JsonProperty("meetingCreatedCount")]
    public int MeetingCreated { get; set; }

    [JsonProperty("meetingInteractedCount")]
    public int MeetingInteracted { get; set; }
}
