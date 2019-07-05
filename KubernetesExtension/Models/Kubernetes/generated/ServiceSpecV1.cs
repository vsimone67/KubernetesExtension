using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace KubeClient.Models
{
    /// <summary>
    ///     ServiceSpec describes the attributes that a user creates on a service.
    /// </summary>
    public partial class ServiceSpecV1
    {
        /// <summary>
        ///     clusterIP is the IP address of the service and is usually assigned randomly by the master. If an address is specified manually and is not in use by others, it will be allocated to the service; otherwise, creation of the service will fail. This field can not be changed through updates. Valid values are "None", empty string (""), or a valid IP address. "None" can be specified for headless services when proxying is not required. Only applies to types ClusterIP, NodePort, and LoadBalancer. Ignored if type is ExternalName. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies
        /// </summary>
        [YamlMember(Alias = "clusterIP")]
        [JsonProperty("clusterIP", NullValueHandling = NullValueHandling.Ignore)]
        public string ClusterIP { get; set; }

        /// <summary>
        ///     Only applies to Service Type: LoadBalancer LoadBalancer will get created with the IP specified in this field. This feature depends on whether the underlying cloud-provider supports specifying the loadBalancerIP when a load balancer is created. This field will be ignored if the cloud-provider does not support the feature.
        /// </summary>
        [YamlMember(Alias = "loadBalancerIP")]
        [JsonProperty("loadBalancerIP", NullValueHandling = NullValueHandling.Ignore)]
        public string LoadBalancerIP { get; set; }

        /// <summary>
        ///     externalName is the external reference that kubedns or equivalent will return as a CNAME record for this service. No proxying will be involved. Must be a valid RFC-1123 hostname (https://tools.ietf.org/html/rfc1123) and requires Type to be ExternalName.
        /// </summary>
        [YamlMember(Alias = "externalName")]
        [JsonProperty("externalName", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalName { get; set; }

        /// <summary>
        ///     type determines how the Service is exposed. Defaults to ClusterIP. Valid options are ExternalName, ClusterIP, NodePort, and LoadBalancer. "ExternalName" maps to the specified externalName. "ClusterIP" allocates a cluster-internal IP address for load-balancing to endpoints. Endpoints are determined by the selector or if that is not specified, by manual construction of an Endpoints object. If clusterIP is "None", no virtual IP is allocated and the endpoints are published as a set of endpoints rather than a stable IP. "NodePort" builds on ClusterIP and allocates a port on every node which routes to the clusterIP. "LoadBalancer" builds on NodePort and creates an external load-balancer (if supported in the current cloud) which routes to the clusterIP. More info: https://kubernetes.io/docs/concepts/services-networking/service/#publishing-services---service-types
        /// </summary>
        [YamlMember(Alias = "type")]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        /// <summary>
        ///     sessionAffinityConfig contains the configurations of session affinity.
        /// </summary>
        [YamlMember(Alias = "sessionAffinityConfig")]
        [JsonProperty("sessionAffinityConfig", NullValueHandling = NullValueHandling.Ignore)]
        public SessionAffinityConfigV1 SessionAffinityConfig { get; set; }

        /// <summary>
        ///     Route service traffic to pods with label keys and values matching this selector. If empty or not present, the service is assumed to have an external process managing its endpoints, which Kubernetes will not modify. Only applies to types ClusterIP, NodePort, and LoadBalancer. Ignored if type is ExternalName. More info: https://kubernetes.io/docs/concepts/services-networking/service/
        /// </summary>
        [YamlMember(Alias = "selector")]
        [JsonProperty("selector", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public Dictionary<string, string> Selector { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     Determine whether the <see cref="Selector"/> property should be serialised.
        /// </summary>
        public bool ShouldSerializeSelector() => Selector.Count > 0;

        /// <summary>
        ///     externalIPs is a list of IP addresses for which nodes in the cluster will also accept traffic for this service.  These IPs are not managed by Kubernetes.  The user is responsible for ensuring that traffic arrives at a node with this IP.  A common example is external load-balancers that are not part of the Kubernetes system.
        /// </summary>
        [YamlMember(Alias = "externalIPs")]
        [JsonProperty("externalIPs", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<string> ExternalIPs { get; } = new List<string>();

        /// <summary>
        ///     Determine whether the <see cref="ExternalIPs"/> property should be serialised.
        /// </summary>
        public bool ShouldSerializeExternalIPs() => ExternalIPs.Count > 0;

        /// <summary>
        ///     If specified and supported by the platform, this will restrict traffic through the cloud-provider load-balancer will be restricted to the specified client IPs. This field will be ignored if the cloud-provider does not support the feature." More info: https://kubernetes.io/docs/tasks/access-application-cluster/configure-cloud-provider-firewall/
        /// </summary>
        [YamlMember(Alias = "loadBalancerSourceRanges")]
        [JsonProperty("loadBalancerSourceRanges", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<string> LoadBalancerSourceRanges { get; } = new List<string>();

        /// <summary>
        ///     Determine whether the <see cref="LoadBalancerSourceRanges"/> property should be serialised.
        /// </summary>
        public bool ShouldSerializeLoadBalancerSourceRanges() => LoadBalancerSourceRanges.Count > 0;

        /// <summary>
        ///     The list of ports that are exposed by this service. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies
        /// </summary>
        [MergeStrategy(Key = "port")]
        [YamlMember(Alias = "ports")]
        [JsonProperty("ports", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<ServicePortV1> Ports { get; } = new List<ServicePortV1>();

        /// <summary>
        ///     Determine whether the <see cref="Ports"/> property should be serialised.
        /// </summary>
        public bool ShouldSerializePorts() => Ports.Count > 0;

        /// <summary>
        ///     publishNotReadyAddresses, when set to true, indicates that DNS implementations must publish the notReadyAddresses of subsets for the Endpoints associated with the Service. The default value is false. The primary use case for setting this field is to use a StatefulSet's Headless Service to propagate SRV records for its Pods without respect to their readiness for purpose of peer discovery.
        /// </summary>
        [YamlMember(Alias = "publishNotReadyAddresses")]
        [JsonProperty("publishNotReadyAddresses", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PublishNotReadyAddresses { get; set; }

        /// <summary>
        ///     healthCheckNodePort specifies the healthcheck nodePort for the service. If not specified, HealthCheckNodePort is created by the service api backend with the allocated nodePort. Will use user-specified nodePort value if specified by the client. Only effects when Type is set to LoadBalancer and ExternalTrafficPolicy is set to Local.
        /// </summary>
        [YamlMember(Alias = "healthCheckNodePort")]
        [JsonProperty("healthCheckNodePort", NullValueHandling = NullValueHandling.Ignore)]
        public int? HealthCheckNodePort { get; set; }

        /// <summary>
        ///     externalTrafficPolicy denotes if this Service desires to route external traffic to node-local or cluster-wide endpoints. "Local" preserves the client source IP and avoids a second hop for LoadBalancer and Nodeport type services, but risks potentially imbalanced traffic spreading. "Cluster" obscures the client source IP and may cause a second hop to another node, but should have good overall load-spreading.
        /// </summary>
        [YamlMember(Alias = "externalTrafficPolicy")]
        [JsonProperty("externalTrafficPolicy", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalTrafficPolicy { get; set; }

        /// <summary>
        ///     Supports "ClientIP" and "None". Used to maintain session affinity. Enable client IP based session affinity. Must be ClientIP or None. Defaults to None. More info: https://kubernetes.io/docs/concepts/services-networking/service/#virtual-ips-and-service-proxies
        /// </summary>
        [YamlMember(Alias = "sessionAffinity")]
        [JsonProperty("sessionAffinity", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionAffinity { get; set; }
    }
}