using EnvDTE;
using System;

namespace KubernetesExtension.DeployMents
{
    public class HelmDeploy : DeployBase, IDeployment
    {
        public void BuildAndDeployToCluster(KubernetesExtensionPackage package, string dockerHubUserName, string deployName)
        {
            throw new NotImplementedException();
        }

        public void BuildAndDeployToCluster(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public void CheckDeploymentStatus(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public void CreateDeploymentFiles(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public void DeleteDeployment(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public void DeployToCluster(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymentConfiguration(Project project)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymentConfiguration(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymnet(Project project)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymnet(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }

        public void RemoveDeploymentFiles(KubernetesExtensionPackage package)
        {
            throw new NotImplementedException();
        }
    }
}