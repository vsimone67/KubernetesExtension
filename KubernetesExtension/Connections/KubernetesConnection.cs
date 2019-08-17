using KubeClient.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Kubernetes
{
    public class KuberntesConnection : ProcessBase
    {
        // URL TO API DOCS https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.10/#list-28
        public NodeListV1 GetNodes()
        {
            return GetKuberntesApi<NodeListV1>("get --raw /api/v1/nodes");
        }

        public NodeV1 GetNode(string nodeName)
        {
            return GetKuberntesApi<NodeV1>($"get --raw /api/v1/nodes/{nodeName}");
        }

        public NodeV1 GetNodeStatus(string nodeName)
        {
            return GetKuberntesApi<NodeV1>($"get --raw /api/v1/nodes/{nodeName}/status");
        }

        public NamespaceListV1 GetNameSpaces()
        {
            return GetKuberntesApi<NamespaceListV1>("get --raw /api/v1/namespaces");
        }

        public DeploymentListV1 GetDeployments(string @namespace)
        {
            return GetKuberntesApi<DeploymentListV1>($"get --raw /apis/apps/v1/namespaces/{@namespace}/deployments");
        }

        public DeploymentListV1 GetAllDeployments()
        {
            return GetKuberntesApi<DeploymentListV1>($"get deployment --all-namespaces -o=json");
            
        }

        public DeploymentV1 GetDeploymentInfo(string deployment, string @nameSpace)
        {
            var deplopymentList = GetDeployments(@nameSpace);

            var deplopment = deplopymentList.Where(exp => exp.Metadata.Name == deployment).FirstOrDefault();

            return deplopment;
        }

        public ConfigMapListV1 GetConfigMaps(string @namespace)
        {
            return GetKuberntesApi<ConfigMapListV1>($"get --raw /api/v1/namespaces/{@namespace}/configmaps");
        }

        public SecretListV1 GetSecrets(string @namespace)
        {
            return GetKuberntesApi<SecretListV1>($"get --raw /api/v1/namespaces/{@namespace}/secrets");
        }

        public ServiceListV1 GetServices(string @namespace)
        {
            return GetKuberntesApi<ServiceListV1>($"get --raw /api/v1/namespaces/{@namespace}/services");
        }

        public ServiceV1 GetServices(string @namespace, string serviceName)
        {
            return GetKuberntesApi<ServiceV1>($"get --raw /api/v1/namespaces/{@namespace}/services/{serviceName}");
        }

        public PodListV1 GetPods(string @namespace)
        {
            return GetKuberntesApi<PodListV1>($"get --raw /api/v1/namespaces/{@namespace}/pods");
        }

        public PodListV1 GetAllPods()
        {
            return GetKuberntesApi<PodListV1>($"get --raw /api/v1/pods");
        }

        public PodStatusV1 GetPodStatus(string @namespace, string podName)
        {
            return GetKuberntesApi<PodStatusV1>($"get --raw /api/v1/namespaces/{@namespace}/pods/{podName}/status");
        }

        public ConifgView GetKubeConfigs()
        {
            return GetKuberntesApi<ConifgView>("config view -o json");
        }

        public PodListV1 GetPodsRunningOnNode(string nodeName, string @namespace)
        {
            return GetKuberntesApi<PodListV1>($"get pods --field-selector=spec.nodeName={nodeName} --namespace {@namespace} -o json");
        }

        public void ScaleDeployment(string deploymentname, int numberOfReplicas, string kNameSpace)
        {
            GetKuberntesApi($"scale --replicas {numberOfReplicas} deployment/{deploymentname} --namespace {kNameSpace}");
        }

        public void DeployToCluster(string yamlFile, string path)
        {
            string command = $"apply -f {path}\\{yamlFile}";

            RunKubernetsCommand(command);
        }

        public void RemoveFromCluster(string yamlFile, string path)
        {
            string command = $"delete -f {path}\\{yamlFile}";

            RunKubernetsCommand(command);
        }

        public void RemoveDeployment(string deployment)
        {
            string command = $"delete deployment/{deployment}";

            RunKubernetsCommand(command);
        }

        protected T GetKuberntesApi<T>(string command)
        {
            RunKubernetsCommand(command);

            return JsonConvert.DeserializeObject<T>(json);
        }

        protected void GetKuberntesApi(string command)
        {
            RunKubernetsCommand(command);
        }

        protected void RunKubernetsCommand(string command)
        {
            RunCommand("kubectl.exe", command);
        }

        private string GetK8sConfigPath()
        {
            var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            return System.IO.Path.Combine(home, @".kube\config");
        }

        private YamlStream LoadKubeConfig()
        {
            var k8sConfigPath = GetK8sConfigPath();
            if (!System.IO.File.Exists(k8sConfigPath))
                return null;

            using (var r = System.IO.File.OpenText(k8sConfigPath))
            {
                var yaml = new YamlStream();
                yaml.Load(r);
                return yaml;
            }
        }

        public string GetCurrentContext()
        {
            var yaml = LoadKubeConfig();
            if (yaml == null)
                return null;

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            return ((YamlScalarNode)mapping["current-context"]).Value;
        }

        public string[] GetContextNames()
        {
            IList<string> l = new List<string>();

            var yaml = LoadKubeConfig();
            if (yaml != null)
            {
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                var contexts = (YamlSequenceNode)mapping.Children[new YamlScalarNode("contexts")];
                foreach (YamlMappingNode context in contexts)
                {
                    l.Add(context["name"].ToString());
                }
            }

            return l.ToArray();
        }
    }
}