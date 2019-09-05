// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FamilyListView.xaml.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The family list window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.V
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    using AddFamilyParameters.M;
    using AddFamilyParameters.Static_classes;
    using AddFamilyParameters.Utilities;
    using AddFamilyParameters.VM;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Control = System.Windows.Controls.Control;

    /// <summary>
    /// The family list window.
    /// </summary>
    public partial class FamilyListView : Window
    {
        private static bool isAddSharedChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyListView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="isAddShared"></param>
        public FamilyListView(Document doc, bool isAddShared)
        {
            try
            {
                this.InitializeComponent();
                isAddSharedChecked = isAddShared;

                FamilyListViewModel familyListViewModel = new FamilyListViewModel(doc);
                this.Families = familyListViewModel.FamCategoriesList;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Initializing Component", e.Message);
            }
        }

        /// <summary>
        /// Gets or sets the families.
        /// </summary>
        public ObservableCollection<FamilyCategory> Families { get; set; }

        private void ButtonLoadParametersClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FamilyListViewModel.AddFamilyParameters(this.Families, isAddSharedChecked, this.CheckBoxProceedProjectParameters.IsChecked ?? false);
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Add Family Parameters", exception.Message);
            }
        }

        private void ButtonUncheckAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var familyCategory in this.Families)
            {
                ItemHelper.SetIsChecked(familyCategory, false);
                foreach (var family in familyCategory.Members)
                {
                    ItemHelper.SetIsChecked(family, false);
                }
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonExpandAll_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (object item in this.FamiliesTreeView.Items)
            {
                TreeViewItem treeItem = this.FamiliesTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    this.ExpandAll(treeItem, true);
                }

                treeItem.IsExpanded = true;
            }
        }

        private void ButtonCollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (object item in this.FamiliesTreeView.Items)
            {
                TreeViewItem treeItem = this.FamiliesTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    this.ExpandAll(treeItem, false);
                }

                treeItem.IsExpanded = false;
            }
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    this.ExpandAll(childControl, expand);
                }

                if (childControl is TreeViewItem item)
                {
                    item.IsExpanded = true;
                }
            }
        }
    }
}