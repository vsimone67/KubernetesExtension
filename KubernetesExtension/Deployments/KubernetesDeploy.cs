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
            _package = package;
            var appName = MakeDeploymentName(package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            BuildDockerandPublishDockerImage(appName, projectDir);
        }

        public void CreateDeploymentFiles(KubernetesExtensionPackage package)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _package = package;
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            Directory.CreateDirectory($"{projectDir}\\{kubeName}");
            File.WriteAllText($"{projectDir}\\{kubeName}\\deployment.yaml", GetKubeYamlText().Replace("NAMEGOESHERE", MakeDeploymentName(package.GetCurrentProject().Name)));
            File.WriteAllText($"{projectDir}\\{kubeName}\\createconfigs.ps1", GetSettingsForScript().Replace("NAMEGOESHERE", MakeDeploymentName(package.GetCurrentProject().Name)));
            File.WriteAllText($"{projectDir}\\{kubeName}\\deploy.ps1", GetPowerShellDeployScript());
            package.GetCurrentProject().ProjectItems.AddFolder($"{projectDir}\\{kubeName}");
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
            _package = package;

            var item = GetProjectItem(_package.GetCurrentProject().ProjectItems, kubeName);
            return (item != null && VSConstants.GUID_ItemType_PhysicalFolder == new Guid(item.Kind));
        }

        public bool HasDeploymnet(KubernetesExtensionPackage package)
        {
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
            _package = package;

            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{kubeName}";
            var kubeCommand = "delete -f deployment.yaml";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        public void CheckDeploymentStatus(KubernetesExtensionPackage package)
        {            
            _package = package;
            
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{kubeName}";
            var knamespace = GetNameSpaceFromYaml();         
            var kubeCommand = $"rollout status deploy/{appName} --namespace {knamespace}";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }
        protected void DeployAllToCluster(KubernetesExtensionPackage package)
        {
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{kubeName}";
            var kubeCommand = "apply -f deployment.yaml";

            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        protected void UpdateDeployment(KubernetesExtensionPackage package)
        {
            _package = package;
            //kubectl set image deployment testdeploy testdeploy-pod=vsimone67/testdeploy --namespace playground
            var appName = MakeDeploymentName(_package.GetCurrentProject().Name);
            var projectDir = Path.GetDirectoryName(package.GetCurrentProject().FullName);
            var yamlDir = $"{projectDir}\\{kubeName}";
            var knamespace = GetNameSpaceFromYaml();
            var kubeCommand = $"set image deployment {appName} {appName}-pod={dockerHubUserName}/{appName}:latest --namespace {knamespace}";
            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, true, Process_OutputDataReceived, Process_ErrorDataReceived);
            kubeCommand = $"set image deployment {appName} {appName}-pod={dockerHubUserName}/{appName} --namespace {knamespace}";
            Utils.RunProcess("kubectl.exe", kubeCommand, yamlDir, false, Process_OutputDataReceived, Process_ErrorDataReceived);
        }

        private string GetNameSpaceFromYaml()
        {
            var retval = string.Empty;

            var projectDir = Path.GetDirectoryName(_package.GetCurrentProject().FullName);
            var filename = $"{projectDir}\\{kubeName}\\deployment.yaml";

            GetValueFromFile file = new GetValueFromFile();
            retval = file.GetValue(filename, "namespace");

            return retval;
        }

        public void RemoveDeploymentFiles(KubernetesExtensionPackage package)
        {
            var project = package.GetCurrentProject();
            var projectDir = Path.GetDirectoryName(project.FullName);
            var item = GetProjectItem(project.ProjectItems, kubeName);
            item.Delete();
        }

        protected override void Process_DockerBuildComplete(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Delay(5000).Wait();
            DeployToCluster(_package);
            int x = 0;
        }

        #region Yaml/PS file contents

        private string GetKubeYamlText()
        {
            return @"apiVersion: v1
kind: Namespace
metadata:
  name: NAMESPACEGOESHERE
---
apiVersion: extensions/v1beta1
kind: Deployment
metadata:
  name: NAMEGOESHERE
  namespace: NAMESPACEGOESHERE
spec:
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
          env:
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
                      name: appsettings-secret-NAMEGOESHERE
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
        }

        protected string GetPowerShellDeployScript()
        {
            return @"param(
		[Parameter(Position=0, Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[System.String]
		$appName,

		[Parameter(Position=1, Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[System.String]
		$projectDir
	)

Set-Location $projectDir.Substring(0,$projectDir.LastIndexOf(""\""))
docker build -t $appName -f ""$($projectDir)\dockerfile"".
docker tag ""$($appName):latest"" vsimone67/""$($appName):latest""
docker push vsimone67/""$($appName):latest""";
        }

        protected string GetSettingsForScript()
        {
            return @"kubectl create secret generic appsettings-secret-NAMEGOESHERE --namespace NAMESPACEGOESHERE --from-file=./appsettings.secrets.json

kubectl create configmap appsettings-NAMEGOESHERE --namespace NAMESPACEGOESHERE --from-file=./appsettings.json";
        }

        
    }

    #endregion Yaml/PS file contents
}