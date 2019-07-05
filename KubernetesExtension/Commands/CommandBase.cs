using System;

namespace KubernetesExtension.Commands
{
    public class CommandBase
    {
        protected static readonly Guid CommandSet = new Guid("ffd7b322-cdf7-4851-9677-4569e4f60c0a");

        protected KubernetesExtensionPackage _package;
        protected bool _isProcessRunning;
        protected IDeployment _deployment;
        public CommandBase()
        {
            _isProcessRunning = false;            
        }

        protected string NormalizeAppName(string name)
        {
            return name.Replace(" ", "_").ToLower();
        }

        protected Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }
    }
}