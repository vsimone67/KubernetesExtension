using KubeClient.Models;
using System.Collections.ObjectModel;

namespace Kubernetes.Models
{
    public class NodeInfo
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Cidr { get; set; }
        public bool IsRunning { get; set; }
        public string StatusMessage { get; set; }
        public ObservableCollection<PodV1> PodList { get; set; }
        public NodeSystemInfoV1 NodeSystemInfo { get; set; }

        public NodeInfo()
        {
            PodList = new ObservableCollection<PodV1>();
        }
    }
}