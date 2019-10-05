using KubeClient.Models;
using Kubernetes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;

namespace KubernetesExtension
{
    public class KubernetesDeploy : DeployBase, IDeployment
    {
        protected string _projectDir;

        public void BuildAndDeployToCluster(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var appName = MakeDeploymentName(package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            BuildDockerandPublishDockerImage(appName, projectDir, _package.GetKubeOptions().KubeDir);
        }

        public void CreateDeploymentFiles(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            Directory.CreateDirectory($"{projectDir}\\{_package.GetKubeOptions().KubeDir}");

            string deployYaml = GetKubeYamlText(_package.GetKubeOptions().AddConfig);
            string configYaml = GetSettingsForScript();
            deployYaml = deployYaml.Replace("NAMEGOESHERE", MakeDeploymentName(package.GetCurrentProject().Name));
            deployYaml = deployYaml.Replace("NAMESPACEGOESHERE", _package.GetKubeOptions().KubernetesNamespace);

            configYaml = configYaml.Replace("NAMEGOESHERE", MakeDeploymentName(package.GetCurrentProject().Name));
            configYaml = configYaml.Replace("NAMESPACEGOESHERE", _package.GetKubeOptions().KubernetesNamespace);

            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\deployment.yaml", deployYaml);
            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\createconfigs.ps1", configYaml);
            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\deploy.ps1", GetPowerShellDeployScript());
            File.WriteAllText($"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\createnamespace.ps1", GetNamespaceScript());
            package.GetCurrentProject().ProjectItems.AddFolder($"{projectDir}\\{_package.GetKubeOptions().KubeDir}");
        }

        public async void DeployToCluster(KubernetesExtensionPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _package = package;

            if (!HasDeploymnet(_package))
            {
                DeployAllToCluster(_package);
            }
            else
            {
                UpdateDeployment(_package);
            }
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
            bool retval = false;
            KuberntesConnection kubeConnection = new KuberntesConnection();
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            var deployments = kubeConnection.GetAllDeployments();

            retval = deployments.Items.Any(exp => exp.Metadata.Name.ToUpper() == appName.ToUpper());
            return retval;
        }

        public void DeleteDeployment(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{_package.GetKubeOptions().KubeDir}";
            var kubeCommand = "delete -f deployment.yaml";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        public void CheckDeploymentStatus(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{_package.GetKubeOptions().KubeDir}";
            var knamespace = GetNameSpaceFromYaml();
            var kubeCommand = $"rollout status deploy/{appName} --namespace {knamespace}";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
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

        protected void DeployAllToCluster(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{_package.GetKubeOptions().KubeDir}";
            var kubeCommand = "apply -f deployment.yaml";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        protected void UpdateDeployment(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;

            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{_package.GetKubeOptions().KubeDir}";
            var knamespace = GetNameSpaceFromYaml();
            var kubeCommand = $"set image deployment {appName} {appName}-pod={_package.GetKubeOptions().DockerHubAccount}/{appName}:latest --namespace {knamespace}";
            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, true, Process_OutputDataReceived, Process_ErrorDataReceived);
            kubeCommand = $"set image deployment {appName} {appName}-pod={_package.GetKubeOptions().DockerHubAccount}/{appName} --namespace {knamespace}";
            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
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

        protected override void Process_DockerBuildComplete(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Delay(5000).Wait();
            DeployToCluster(_package);
        }

        #region Yaml/PS file contents

        private string GetNamespaceScript()
        {
            return $"kubectl create namespace { _package.GetKubeOptions().KubernetesNamespace}";
        }

        private string GetKubeYamlText(bool addConfig)
        {
            string header = @"apiVersion: v1
kind: Namespace
metadata:
  name: NAMESPACEGOESHERE
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: NAMEGOESHERE
  namespace: NAMESPACEGOESHERE
spec:
  selector:
    matchLabels:
      app: NAMEGOESHERE
  replicas: 1
  minReadySeconds: 10
  template:
    metadata:
      labels:
        app: NAMEGOESHERE
    spec:
      containers:
        - name: NAMEGOESHERE-pod
          image: vsimone67/NAMEGOESHERE:latest
          imagePullPolicy: ""Always""
          ports:
            - name: http
              containerPort: 80
";
            string configMaps = @"          env:
            - name: ""appdirectory""
              value: ""/app/settings/""
          volumeMounts:
            - name: configs
              mountPath: ""/app/settings""
      volumes:
            - name: configs
              projected:
                sources:
                  - configMap:
                      name: appsettings-NAMEGOESHERE
                  - secret:
                      name: appsettings-secret-NAMEGOESHERE";

            string service = @"
---
apiVersion: v1
kind: Service
metadata:
  name: NAMEGOESHERE-svc
  namespace: NAMESPACEGOESHERE
spec:
  ports:
    - name: http
      port: 80
      protocol: TCP
      targetPort: 80
  selector:
    app: NAMEGOESHERE
  type: LoadBalancer";

            return header + ((addConfig) ? configMaps : "") + service;
        }
    }

    #endregion Yaml/PS file contents
}