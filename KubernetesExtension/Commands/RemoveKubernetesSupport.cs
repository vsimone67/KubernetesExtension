using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace KubernetesExtension.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RemoveKubernetesSupport : CommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveKubernetesSupport"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RemoveKubernetesSupport(KubernetesExtensionPackage package, OleMenuCommandService commandService)
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
            item.Enabled = !_isProcessRunning && _deployment.HasDeploymentConfiguration(_package);
            item.Visible = true;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RemoveKubernetesSupport Instance
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
            // Switch to the main thread - the call to AddCommand in RemoveKubernetesSupport's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RemoveKubernetesSupport(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            _isProcessRunning = true;
            _deployment.RemoveDeploymentFiles(_package);
            _isProcessRunning = false;
        }
    }
}