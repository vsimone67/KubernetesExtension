﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;

namespace KubernetesExtension
{
    public class DeployBase
    {
        protected KubernetesExtensionPackage _package;

        public static ProjectItem GetProjectItem(ProjectItems items, string name)
        {
            foreach (ProjectItem p in items)
                if (p.Name == name)
                    return p;
            return null;
        }

        protected string NormalizeAppName(string name)
        {
            return name.Replace(" ", "_").ToLower();
        }

        protected string AddDeploymnetType(string name, string prefix = "")
        {
            string newName = name;

            if (name.ToUpper().Contains("SERVICE"))
            {
                newName += $"{prefix}service";
            }

            if (name.ToUpper().Contains("WEB") || name.ToUpper().Contains("PRESENTATION"))
            {
                newName += $"{prefix}web";
            }

            if (name.ToUpper().Contains("API"))
            {
                newName += $"{prefix}gateway";
            }

            return newName;
        }

        protected string GetDeploymentName(string name)
        {
            name = NormalizeAppName(name);
            string newName = name;

            // remove any project portions of the name e.g. project.domain.name
            if (name.Contains("."))
            {
                // just get the right most portion of the project
                int pos = name.LastIndexOf(".");
                newName = name.Substring(pos + 1);
            }

            return newName;
        }

        protected string MakeDeploymentName(string name)
        {
            string newName = AddDeploymnetType(name);
            return GetDeploymentName(newName);
        }

        protected async void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (e.Data != null)
            {
                _package.WriteToOutputWindow(e.Data);
            }
        }

        protected async void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (e.Data != null)
            {
                _package.WriteToOutputWindow(e.Data);
            }
        }

        protected void BuildDockerandPublishDockerImage(string appName, string projectDir, string deployDir)
        {
            var psCommand = $"./deploy.ps1 -appName {appName} -projectDir {projectDir}";
            var psDir = $"{projectDir}\\{deployDir}";
            Utils.RunProcess("powershell.exe", psCommand, psDir, false, Process_OutputDataReceived, Process_ErrorDataReceived, Process_DockerBuildComplete);
        }

        protected string GetNameSpaceFromYaml()
        {
            var retval = string.Empty;

            var projectDir = Path.GetDirectoryName(_package.GetCurrentProject().FullName);
            var filename = $"{projectDir}\\{_package.GetKubeOptions().KubeDir}\\deployment.yaml";

            GetValueFromFile file = new GetValueFromFile();
            retval = file.GetValue(filename, "namespace");

            return retval;
        }

        protected virtual void Process_DockerBuildComplete(object sender, EventArgs e)
        {
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
            return @"kubectl create secret generic appsettings-secret-NAMEGOESHERE --namespace NAMESPACEGOESHERE --from-file=../appsettings.secrets.json

kubectl create configmap appsettings-NAMEGOESHERE --namespace NAMESPACEGOESHERE --from-file=../appsettings.json";
        }
    }
}