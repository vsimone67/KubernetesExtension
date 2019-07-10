using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using KubernetesExtension.Commands;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SimpleInjector;
using Task = System.Threading.Tasks.Task;

namespace KubernetesExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(KubernetesExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)] // Trigger CodeMaid to load on solution open so menu items can determine their state.
    [ProvideToolWindow(typeof(KubernetesExtension.Windows.KubernetesCluster))]
    public sealed class KubernetesExtensionPackage : AsyncPackage
    {
        /// <summary>
        /// KubernetesExtensionPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "217f3169-2ead-43ea-afc5-ba2e93b6a8a5";
        private static readonly Guid kubernetesPaneGuid = new Guid("2BE8BB60-E918-4C59-8717-B078A6927D34");
        private Container _iocContainer;

        public IVsStatusbar GetStatusBar()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (IVsStatusbar)GetService(typeof(SVsStatusbar));
        }
        
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            _iocContainer = new Container();

            RegisterComponents();
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await AddKubernetesSupport.InitializeAsync(this);
            await BuildAndDeployToCluster.InitializeAsync(this);
            await KubernetesExtension.Commands.RemoveKubernetesSupport.InitializeAsync(this);
            await KubernetesExtension.Commands.DeployToCluster.InitializeAsync(this);
            await KubernetesExtension.Commands.DeleteDeployment.InitializeAsync(this);
            await KubernetesExtension.Commands.CheckDeploymentStatus.InitializeAsync(this);
            await KubernetesExtension.Windows.KubernetesClusterCommand.InitializeAsync(this);
        }

        private void RegisterComponents()
        {
            _iocContainer.Register<IDeployment, KubernetesDeploy>();
        }

        public TService GetService<TService>() 
        {
            return (TService)_iocContainer.GetInstance(typeof(TService)) ;
            
        }

        #endregion Package Members

        public void SetStatusBarText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            GetStatusBar().SetText(text);
        }

        public string GetVSExtensionDir()
        {
            var extensionPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetDirectoryName(extensionPath);
        }

        public string GetVSExtensionFilePath(string relPath)
        {
            return Path.Combine(GetVSExtensionDir(), relPath);
        }

        public Project GetCurrentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE service = (DTE)GetService(typeof(DTE));
            Assumes.Present(service);
            var activeSolutionProjects = (Array)service.ActiveSolutionProjects;
            return (Project)activeSolutionProjects.GetValue(0);
        }

        public ProjectItem GetProjectItem(Project project, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetProjectItem(project.ProjectItems, name);
        }

        public ProjectItem GetProjectItem(ProjectItems items, string name)
        {
            foreach (ProjectItem p in items)
                if (p.Name == name)
                    return p;
            return null;
        }

        public bool ProjectExists(Solution solution, string projectName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (Project p in solution.Projects)
                if (p.Name == projectName)
                    return true;
            return false;
        }       

        public  void ShowWarningMessageBox(string message, string title = "Kubernetes for Visual Studio")
        {
            var service = this as IServiceProvider;

            VsShellUtilities.ShowMessageBox(
                service,
                message,
                title,
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public IVsOutputWindowPane GetOutputPane(Guid paneGuid, string title, bool visible, bool clearWithSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsOutputWindow output = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
            Assumes.Present(output);
            IVsOutputWindowPane pane;
            output.CreatePane(ref paneGuid, title, Convert.ToInt32(visible), Convert.ToInt32(clearWithSolution));
            var hr = output.GetPane(ref paneGuid, out pane);
            ErrorHandler.ThrowOnFailure(hr);
            return pane;
        }

        public void WriteToOutputWindow(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var paneGuid = kubernetesPaneGuid;
            var pane = GetOutputPane(paneGuid, "Kubernetes", true, false);
            pane.Activate();
            pane.OutputString(message + "\n");
        }

        public void AddItemsToProject(ProjectItems projectItems, IEnumerable<string> paths)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var path in paths)
            {
                try
                {
                    var itemName = Path.GetFileName(path);
                    if (File.Exists(path))
                    {
                        if (GetProjectItem(projectItems, itemName) == null)
                            projectItems.AddFromFile(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        var childProjectItem = GetProjectItem(projectItems, itemName);
                        if (childProjectItem == null)
                            childProjectItem = projectItems.AddFromDirectory(path);
                        var childPaths = Directory.GetFileSystemEntries(path);
                        AddItemsToProject(childProjectItem.ProjectItems, childPaths);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Item exists, ignore exception
                }
            }
        }

        public void RemovetemsToProject(ProjectItems projectItems, string fullName)
        {
            try
            {
                var itemName = Path.GetFileName(fullName);
                if (File.Exists(fullName))
                {
                    var projectItem = GetProjectItem(projectItems, itemName);

                    if (projectItem != null)
                    {
                        projectItem.Delete();
                        File.Delete(fullName);
                    }
                }
                else if (Directory.Exists(fullName))
                {
                    var childProjectItem = GetProjectItem(projectItems, itemName);
                    if (childProjectItem != null)
                        childProjectItem.Delete();
                }
            }
            catch (InvalidOperationException)
            {
                // Item exists, ignore exception
            }
        }
    }
}