using Newtonsoft.Json;

namespace ActivityImporter.Engine.ActivityAPI.Models
{
    public abstract class BaseGraphSerialisationClass
    {

        [JsonProperty("id")]
        public string GraphCallID { get; set; } = null!;
    }
}
