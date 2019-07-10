namespace KubernetesExtension.Windows
{
    using Helm.Release;
    using KubeClient.Models;
    using Kubernetes.ViewModels;
    using Kubernetes.Windows;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Interaction logic for KubernetesClusterControl.
    /// </summary>
    public partial class KubernetesClusterControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClusterControl"/> class.
        /// </summary>
        public KubernetesClusterControl()
        {
            this.InitializeComponent();
            DataContext = new KubernetesViewModel();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
            var namespaces = kbModel.GetNameSpaces();

            List<ObjectMetaV1> data = new List<ObjectMetaV1>();

            foreach (var @namespace in namespaces)
            {
                data.Add(@namespace.Metadata);
            }

            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            comboBox.DisplayMemberPath = "Name";
            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;
            ObjectMetaV1 selectedNameSpace = comboBox.SelectedItem as ObjectMetaV1;

            if (selectedNameSpace != null)
            {
                KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
                kbModel.SelectedNameSpace = selectedNameSpace;
                kbModel.LoadItems(selectedNameSpace.Name);
            }
        }

        private void ScaleDeployment(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null)
            {
                KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
                var dialog = new DeploymentDialog(kbModel.SelectedKubernetesDeployment);

                if (dialog.ShowDialog() == true)
                {
                    kbModel.ScaleDeployment(kbModel.SelectedKubernetesDeployment.Metadata.Name, dialog.NumberOfReplicas);
                }

                e.Handled = true;
            }
        }

        private void DeleteDeployment(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
            {
                KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
                if (MessageBox.Show($"Remove Helm Release {kbModel.SelectedHelmDeployment.Name}?", "Remove Helm Chart", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    kbModel.DeleteHelmDeployment(kbModel.SelectedHelmDeployment.Name);
                }
                e.Handled = true;
            }
        }

        private void DeleteKubeDeployment(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item != null)
            {
                KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
                if (MessageBox.Show($"Remove Helm Release {kbModel.SelectedHelmDeployment.Name}?", "Remove Kubernetes Deployment", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    kbModel.DeleteKubernetesmDeployment(kbModel.SelectedHelmDeployment.Name);
                }
                e.Handled = true;
            }
        }

        

        private void KubernetesDeployment_Selected(object sender, RoutedEventArgs e)
        {
            KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;
            if (kubeclustertree.SelectedItem is DeploymentV1)
                kbModel.SelectedKubernetesDeployment = (DeploymentV1)kubeclustertree.SelectedItem;
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem =
             VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        private void HelmDeployment_Selected(object sender, RoutedEventArgs e)
        {
            KubernetesViewModel kbModel = (KubernetesViewModel)DataContext;

            if (kubeclustertree.SelectedItem is HelmRelease)
                kbModel.SelectedHelmDeployment = (HelmRelease)kubeclustertree.SelectedItem;
        }

        private static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            DependencyObject returnVal = source;

            while (returnVal != null && !(returnVal is T))
            {
                DependencyObject tempReturnVal = null;
                if (returnVal is Visual || returnVal is Visual3D)
                {
                    tempReturnVal = VisualTreeHelper.GetParent(returnVal);
                }
                if (tempReturnVal == null)
                {
                    returnVal = LogicalTreeHelper.GetParent(returnVal);
                }
                else returnVal = tempReturnVal;
            }

            return returnVal as T;
        }
    }
}