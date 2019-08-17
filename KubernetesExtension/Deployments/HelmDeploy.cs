using EnvDTE;
using KubeClient.Models;
using Kubernetes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KubernetesExtension.DeployMents
{
    public class HelmDeploy : DeployBase, IDeployment
    {
        public void BuildAndDeployToCluster(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var appName = MakeDeploymentName(package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            BuildDockerandPublishDockerImage(appName, projectDir, _package.GetKubeOptions().KubeDir);
        }

        public void CheckDeploymentStatus(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            string helmCommand = $"status {appName}";
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            Utils.RunProcess("helm.exe", helmCommand, projectDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        public void CreateDeploymentFiles(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            Utils.RunProcess("helm.exe", $"create {_package.GetKubeOptions().KubeDir}", projectDir, false, Process_OutputDataReceived, Process_ErrorDataReceived, Process_ProcessFinished);
        }      

        public void DeleteDeployment(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            string helmCommand = $"delete --purge {appName}";
            Utils.RunProcess("helm.exe", helmCommand, projectDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        public async void DeployToCluster(KubernetesExtensionPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _package = package;
            var appName = MakeDeploymentName(package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            string commandAction;

            if (!HasDeploymnet(_package))
            {
                commandAction = $"install -n {appName}";
            }
            else
            {
                commandAction = $"upgrade {appName} --recreate-pods";
            }

            string helmCommand = $"{commandAction} .\\{_package.GetKubeOptions().KubeDir}\\";
            Utils.RunProcess("helm.exe", helmCommand, projectDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);            
        }

        public bool HasDeploymentConfiguration(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            var item = GetProjectItem(_package.GetCurrentProject().ProjectItems, _package.GetKubeOptions().KubeDir);
            return (item != null && VSConstants.GUID_ItemType_PhysicalFolder == new Guid(item.Kind));
        }

        public bool HasDeploymnet(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(_package.GetCurrentProject().FullName);
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            return HasHelmDeploy(appName, projectDir);
        }

        public void RemoveDeploymentFiles(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var project = package.GetCurrentProject();
            var projectDir = Path.GetDirectoryName(project.FullName);
            var item = GetProjectItem(project.ProjectItems, _package.GetKubeOptions().KubeDir);
            item.Delete();
        }

        #region Helper Methods

        protected override void Process_DockerBuildComplete(object sender, EventArgs e)
        {            
            System.Threading.Tasks.Task.Delay(5000).Wait();
            DeployToCluster(_package);
        }

        protected async void Process_ProcessFinished(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var projectDir = Path.GetDirectoryName(_package.GetCurrentProject().FullName);
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            ReplaceInFile(System.IO.Path.Combine(projectDir, $"{_package.GetKubeOptions().KubeDir}\\values.yaml"), "nginx", $"{_package.GetKubeOptions().DockerHubAccount}/{appName}");
            ReplaceInFile(System.IO.Path.Combine(projectDir, $"{_package.GetKubeOptions().KubeDir}\\values.yaml"), "stable", "latest");
            ReplaceInFile(System.IO.Path.Combine(projectDir, $"{_package.GetKubeOptions().KubeDir}\\values.yaml"), "IfNotPresent", "Always");
            ReplaceInFile(System.IO.Path.Combine(projectDir, $"{_package.GetKubeOptions().KubeDir}\\values.yaml"), "ClusterIP", "LoadBalancer");
            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\createconfigs.ps1", GetSettingsForScript().Replace("NAMEGOESHERE", MakeDeploymentName(_package.GetCurrentProject().Name)));
            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\deploy.ps1", GetPowerShellDeployScript());
        }

        private void ReplaceInFile(string fileName, string current, string replace)
        {
            string text = File.ReadAllText(fileName);
            text = text.Replace(current, replace);
            File.WriteAllText(fileName, text);
        }

        private void AddToFile(string fileName, string textToAdd)
        {
            File.AppendAllText(fileName,
                   textToAdd + Environment.NewLine);
        }

        public static bool HasHelmDeploy(string appName, string projectDir)
        {
            bool retval = true;
            string capturedProcessOutput = string.Empty;

            Utils.RunProcess("helm.exe", $"history {appName}", projectDir, true, (s, e2) =>
            {
                DataReceivedEventArgs processOutput = (DataReceivedEventArgs)e2;

                if (processOutput.Data != null)
                    capturedProcessOutput += processOutput.Data;
            });

            if (string.IsNullOrEmpty(capturedProcessOutput))
                retval = false;

            return retval;
        }

        public void ScaleDeployment(KubernetesExtensionPackage package, int numberOfReplicas)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            string kNameSpace = GetNameSpaceFromYaml();
            string deploymentName = MakeDeploymentName(package.GetCurrentProject().Name);
            KuberntesConnection kubernetesConnection = new KuberntesConnection();
            kubernetesConnection.ScaleDeployment(deploymentName, numberOfReplicas, kNameSpace);
        }

        public DeploymentV1 GetDeploymentInfo(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            KuberntesConnection kubeConnection = new KuberntesConnection();
            var deployments = kubeConnection.GetAllDeployments();
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            return deployments.Items.Where(exp => exp.Metadata.Name.ToUpper() == appName.ToUpper()).FirstOrDefault();


        }

        #endregion
    }
}