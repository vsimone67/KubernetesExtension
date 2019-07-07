using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;

namespace KubernetesExtension
{
    public class DeployBase
    {
        protected string dockerHubUserName = "vsimone67";
        protected string helmChartName = "charts";        
        protected string kubeName = "k8";
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

        protected void BuildDockerandPublishDockerImage(string appName, string projectDir)
        {           
            var psCommand = $"./deploy.ps1 -appName {appName} -projectDir {projectDir}";
            var psDir = $"{projectDir}\\{kubeName}";
            Utils.RunProcess("powershell.exe", psCommand, psDir, false, Process_OutputDataReceived, Process_ErrorDataReceived, Process_DockerBuildComplete);
        }

        protected virtual void Process_DockerBuildComplete(object sender, EventArgs e)
        {
        }


    }
}