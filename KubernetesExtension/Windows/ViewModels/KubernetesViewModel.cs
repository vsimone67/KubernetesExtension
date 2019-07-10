using Helm.Release;
using KubeClient.Models;
using Kubernetes.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Kubernetes.ViewModels
{
    public class KubernetesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<DeploymentV1> _kubernetesDeployments;
        private ObservableCollection<HelmRelease> _helmDeployments;
        private ObservableCollection<NodeV1> _nodes;
        private ObservableCollection<PodV1> _pods;
        private ObservableCollection<ConfigMapV1> _configMaps;
        private ObservableCollection<SecretV1> _secrets;
        private ObservableCollection<NodeInfo> _nodeInfo;
        private ObservableCollection<ServiceV1> _services;

        public DeploymentV1 SelectedKubernetesDeployment { get; set; }
        public HelmRelease SelectedHelmDeployment { get; set; }

        public ObjectMetaV1 SelectedNameSpace { get; set; }

        public string CurrentKubeContext { get; set; }

        public ICommand RefreshTreeCommand => new CommandHandlerNoParam(() => RefreshTree(), true);

        public KubernetesViewModel()
        {
            _kubernetesDeployments = new ObservableCollection<DeploymentV1>();
            _helmDeployments = new ObservableCollection<HelmRelease>();
            _nodes = new ObservableCollection<NodeV1>();
            _pods = new ObservableCollection<PodV1>();
            _configMaps = new ObservableCollection<ConfigMapV1>();
            _secrets = new ObservableCollection<SecretV1>();
            _nodeInfo = new ObservableCollection<NodeInfo>();
            _services = new ObservableCollection<ServiceV1>();

            _kubernetesDeployments = new ObservableCollection<DeploymentV1>();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<DeploymentV1> KubernetesDeployments
        {
            get { return _kubernetesDeployments; }
            set
            {
                _kubernetesDeployments = value;
            }
        }

        public ObservableCollection<HelmRelease> HelmDeployments
        {
            get { return _helmDeployments; }
            set
            {
                _helmDeployments = value;
            }
        }

        public ObservableCollection<NodeInfo> Nodes
        {
            get { return _nodeInfo; }
            set
            {
                _nodeInfo = value;
            }
        }

        public ObservableCollection<PodV1> Pods
        {
            get { return _pods; }
            set
            {
                _pods = value;
            }
        }

        public ObservableCollection<ServiceV1> Services
        {
            get { return _services; }
            set
            {
                _services = value;
            }
        }

        public ObservableCollection<ConfigMapV1> ConfigMaps
        {
            get { return _configMaps; }
            set
            {
                _configMaps = value;
            }
        }

        public ObservableCollection<SecretV1> Secrets
        {
            get { return _secrets; }
            set
            {
                _secrets = value;
            }
        }

        private void RefreshTree()
        {
            LoadItems(SelectedNameSpace.Name);
        }

        public void LoadItems(string kubeNameSpace)
        {
            KuberntesConnection kubernetesConnection = new KuberntesConnection();
            HelmConnection helmConnection = new HelmConnection();

            var config = kubernetesConnection.GetKubeConfigs();
            CurrentKubeContext = config.CurrentContext;
            OnPropertyChanged("CurrentKubeContext");

            var nodes = kubernetesConnection.GetNodes();
            _nodeInfo.Clear();
            foreach (var node in nodes)
            {
                var newNode = new NodeInfo()
                {
                    Name = node.Metadata.Name,
                    IpAddress = node.Status.Addresses.First().Address,
                    Cidr = node.Spec.PodCIDR,
                    IsRunning = node.Status.Conditions.FirstOrDefault(exp => exp.Type == "Ready").Status == "True",
                    StatusMessage = node.Status.Conditions.FirstOrDefault(exp => exp.Type == "Ready").Message,
                    NodeSystemInfo = node.Status.NodeInfo
                };

                kubernetesConnection.GetPodsRunningOnNode(newNode.Name, kubeNameSpace).Items.ForEach(pod => newNode.PodList.Add(pod));
                _nodeInfo.Add(newNode);
            }

            var kubeDeployments = kubernetesConnection.GetDeployments(kubeNameSpace);

            if (kubeDeployments != null)
            {
                _kubernetesDeployments.Clear();
                kubeDeployments.Items.ForEach(deployment => _kubernetesDeployments.Add(deployment));
            }

            var helmDeployments = helmConnection.GetHelmReleases(kubeNameSpace);

            if (helmDeployments != null)
            {
                _helmDeployments.Clear();
                helmDeployments.Releases.ForEach(helmDeployment => _helmDeployments.Add(helmDeployment));
            }

            var pods = kubernetesConnection.GetPods(kubeNameSpace);

            if (pods != null)
            {
                _pods.Clear();
                pods.Items.ForEach(pod => _pods.Add(pod));
            }

            var services = kubernetesConnection.GetServices(kubeNameSpace);

            if (services != null)
            {
                _services.Clear();
                services.Items.ForEach(service => _services.Add(service));
            }

            var configMaps = kubernetesConnection.GetConfigMaps(kubeNameSpace);

            if (configMaps != null)
            {
                _configMaps.Clear();
                configMaps.Items.ForEach(configMap => _configMaps.Add(configMap));
            }

            var secrets = kubernetesConnection.GetSecrets(kubeNameSpace);

            if (secrets != null)
            {
                _secrets.Clear();
                secrets.Items.ForEach(secret => _secrets.Add(secret));
            }
        }

        public void ScaleDeployment(string deployment, int replicaCount)
        {
            KuberntesConnection kubernetesConnection = new KuberntesConnection();
            kubernetesConnection.ScaleDeployment(deployment, replicaCount);
        }

        public void DeleteHelmDeployment(string deployment)
        {
            HelmConnection helmConnection = new HelmConnection();
            helmConnection.DeleteHelmReleases(deployment);
            _helmDeployments.Remove(SelectedHelmDeployment);
            OnPropertyChanged("HelmDeployments");
        }

        public NamespaceListV1 GetNameSpaces()
        {
            KuberntesConnection kubernetesConnection = new KuberntesConnection();
            return kubernetesConnection.GetNameSpaces();
        }
    }
}