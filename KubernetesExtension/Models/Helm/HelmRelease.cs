using Newtonsoft.Json;

namespace Helm.Release
{
    public class HelmRelease
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Revision")]
        public long Revision { get; set; }

        [JsonProperty("Updated")]
        public string Updated { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Chart")]
        public string Chart { get; set; }

        [JsonProperty("AppVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("Namespace")]
        public string Namespace { get; set; }
    }
}