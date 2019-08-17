using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace KubernetesExtension.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ScaleDeployment : CommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x1028;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleDeployment"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ScaleDeployment(KubernetesExtensionPackage package, OleMenuCommandService commandService)
        {
            this._package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            _deployment = _package.GetService<IDeployment>();
            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += new EventHandler(this.OnBeforeLoad);
            commandService.AddCommand(menuItem);
        }

        private void OnBeforeLoad(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand item = (OleMenuCommand)sender;
            item.Enabled = !_isProcessRunning && _deployment.HasDeploymnet(_package);
            item.Visible = true;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ScaleDeployment Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(KubernetesExtensionPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ScaleDeployment's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ScaleDeployment(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _isProcessRunning = true;
            

            var dialog = new DeploymentDialog(_deployment.GetDeploymentInfo(_package));

            if (dialog.ShowDialog() == true)
            {
                _package.SetStatusBarText("Scaling Deploymnet...");
                _deployment.ScaleDeployment(_package, dialog.NumberOfReplicas);
                _package.SetStatusBarText("Scaling complete...");
            }
            
            _package.SetStatusBarText("");
            _isProcessRunning = false;
        }
    }
}
