using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeClient.Models
{
    public partial class ConifgView
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }

        [JsonProperty("clusters")]
        public List<ClusterElement> Clusters { get; set; }

        [JsonProperty("users")]
        public List<UserElement> Users { get; set; }

        [JsonProperty("contexts")]
        public List<ContextElement> Contexts { get; set; }

        [JsonProperty("current-context")]
        public string CurrentContext { get; set; }
    }

    public partial class ClusterElement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cluster")]
        public ClusterCluster Cluster { get; set; }
    }

    public partial class ClusterCluster
    {
        [JsonProperty("server")]
        public Uri Server { get; set; }

        [JsonProperty("certificate-authority-data")]
        public string CertificateAuthorityData { get; set; }
    }

    public partial class ContextElement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("context")]
        public ContextContext Context { get; set; }
    }

    public partial class ContextContext
    {
        [JsonProperty("cluster")]
        public string Cluster { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }

    public partial class Preferences
    {
    }

    public partial class UserElement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user")]
        public UserUser User { get; set; }
    }

    public partial class UserUser
    {
        [JsonProperty("client-certificate-data")]
        public string ClientCertificateData { get; set; }

        [JsonProperty("client-key-data")]
        public string ClientKeyData { get; set; }
    }
}