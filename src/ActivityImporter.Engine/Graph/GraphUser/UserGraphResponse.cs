using Newtonsoft.Json;

namespace ActivityImporter.Engine.Graph.GraphUser
{
    public class GraphUser
    {
        [JsonProperty("accountEnabled")]
        public bool? AccountEnabled { get; set; }

        [JsonProperty("officeLocation")]
        public string? OfficeLocation { get; set; }

        [JsonProperty("usageLocation")]
        public string? UsageLocation { get; set; }

        [JsonProperty("jobTitle")]
        public string? JobTitle { get; set; }

        [JsonProperty("department")]
        public string? Department { get; set; }

        [JsonProperty("mail")]
        public string? Mail { get; set; }

        [JsonProperty("companyName")]
        public string? CompanyName { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; } = null!;

        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; } = null!;

        [JsonProperty("state")]
        public string? State { get; set; } = null!;

        [JsonProperty("id")]
        public string? Id { get; set; } = null!;

        [JsonProperty("manager@delta")]
        public List<ManagerInfo> ManagerInfo { get; set; } = new();

        [JsonIgnore]
        public ManagerInfo? DefaultManagerInfo => ManagerInfo?.FirstOrDefault();

    }
    public class ManagerInfo
    {
        [JsonProperty("@odata.type")]
        public string? OdataType { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }
    }
}
