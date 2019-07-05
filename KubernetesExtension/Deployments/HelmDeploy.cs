using EnvDTE;
using System;

namespace KubernetesExtension.DeployMents
{
    public class HelmDeploy //: DeployBase, IDeployment
    {
        public void BuildAndDeployToCluster(KubernetesExtensionPackage package, string dockerHubUserName, string deployName)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymentConfiguration(Project project)
        {
            throw new NotImplementedException();
        }

        public bool HasDeploymnet(Project project)
        {
            throw new NotImplementedException();
        }
    }
}