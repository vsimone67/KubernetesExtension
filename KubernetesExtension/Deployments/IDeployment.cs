using EnvDTE;
using KubeClient.Models;
using System.Diagnostics;

namespace KubernetesExtension
{
    public interface IDeployment
    {
        bool HasDeploymentConfiguration(KubernetesExtensionPackage package);

        bool HasDeploymnet(KubernetesExtensionPackage package);

        void CreateDeploymentFiles(KubernetesExtensionPackage package);

        void RemoveDeploymentFiles(KubernetesExtensionPackage package);

        void BuildAndDeployToCluster(KubernetesExtensionPackage package);
        void DeployToCluster(KubernetesExtensionPackage package);

        void DeleteDeployment(KubernetesExtensionPackage package);
        void CheckDeploymentStatus(KubernetesExtensionPackage package);

        void ScaleDeployment(KubernetesExtensionPackage package, int numberOfReplicas);

        DeploymentV1 GetDeploymentInfo(KubernetesExtensionPackage package);

    }
}