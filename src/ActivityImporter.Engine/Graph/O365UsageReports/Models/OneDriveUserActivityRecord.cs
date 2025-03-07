﻿using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.O365UsageReports.Models;

public class OneDriveUserActivityRecord : AbstractUserActivityUserRecordWithUpn
{

    [JsonProperty("viewedOrEditedFileCount")]
    public int ViewedOrEdited { get; set; }

    [JsonProperty("syncedFileCount")]
    public int Synced { get; set; }

    [JsonProperty("sharedInternallyFileCount")]
    public int SharedInternally { get; set; }

    [JsonProperty("sharedExternallyFileCount")]
    public int SharedExternally { get; set; }
}
