using KubeClient.Models;
using System.Windows;

namespace Kubernetes.Windows
{
    /// <summary>
    /// Interaction logic for DeploymentDialog.xaml
    /// </summary>
    public partial class DeploymentDialog : Window
    {
        public DeploymentV1 SelectedDeployment { get; set; }
        public int NumberOfReplicas { get; set; }

        public DeploymentDialog()
        {
            InitializeComponent();
        }

        public DeploymentDialog(DeploymentV1 deployment)
        {
            SelectedDeployment = deployment;
            NumberOfReplicas = SelectedDeployment.Status.Replicas.Value;

            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
            ReplicaCount.Text = NumberOfReplicas.ToString();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            NumberOfReplicas = int.Parse(ReplicaCount.Text);
            this.DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}