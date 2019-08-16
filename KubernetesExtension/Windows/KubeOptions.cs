using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace KubernetesExtension.Windows
{
    public class KubeOptions : DialogPage
    {
        private bool addConfig = true;
        private string kubeDir = "k8";
        private string helmDir = "charts";
        private string dockerHub = "vsimone67";
        private string kubeNameSpace = "fitnesstracker";
        private string deploymentType = "Kubernetes";

        [Category("Kubernetes")]
        [DisplayName("Add Config Maps")]
        [Description("Add Config Maps")]
        public bool AddConfig
        {
            get { return addConfig; }
            set { addConfig = value; }
        }

        [Category("Kubernetes")]
        [DisplayName("Kube Dir")]
        [Description("Kube Dir")]
        public string KubeDir
        {
            get { return kubeDir; }
            set { kubeDir = value; }
        }

        [Category("Kubernetes")]
        [DisplayName("Helm Dir")]
        [Description("Helm Dir")]
        public string HelmDir
        {
            get { return helmDir; }
            set { helmDir = value; }
        }

        [Category("Kubernetes")]
        [DisplayName("Docker Hub Account")]
        [Description("Docker Hub Account")]
        public string DockerHubAccount
        {
            get { return dockerHub; }
            set { dockerHub = value; }
        }

        [Category("Kubernetes")]
        [DisplayName("Kubernetes Namespace")]
        [Description("Kubernetes NameSpace")]
        public string KubernetesNamespace
        {
            get { return kubeNameSpace; }
            set { kubeNameSpace = value; }
        }

        [Category("Kubernetes")]
        [DisplayName("Deployment Type")]
        [Description("Deployment Type (Kubernetes or Helm)")]
        public string DeploymnetType
        {
            get { return deploymentType; }
            set { deploymentType = value; }
        }
    }
}