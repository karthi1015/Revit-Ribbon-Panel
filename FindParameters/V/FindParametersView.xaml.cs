using System;
using System.Windows;

namespace FindParameters.V
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using FindParameters.M;
    using FindParameters.Utilities;
    using FindParameters.VM;

    public partial class FindParametersView : Window
    {
        public FindParametersView(Document doc)
        {
            this.InitializeComponent();

            FindParametersViewModel findParametersViewModel = new FindParametersViewModel(doc);
            this.ParameterGroups = findParametersViewModel.ParameterCategoriesList;
        }

        public ObservableCollection<RevitBuiltInParameterGroup> ParameterGroups { get; set; }

        private void ButtonLoadParametersClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FindParametersViewModel.ExportElementParameters(this.ParameterGroups, this.CheckBoxUseVoid.IsChecked ?? false, this.CheckBoxUseHidden.IsChecked ?? false);
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Add Family Parameters", exception.Message);
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonUncheckAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var familyCategory in this.ParameterGroups)
            {
                ItemHelper.SetIsChecked(familyCategory, false);
                foreach (var family in familyCategory.Members)
                {
                    ItemHelper.SetIsChecked(family, false);
                }
            }
        }

        private void ButtonExpandAll_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (object item in this.CheckBoxParameterCategories.Items)
            {
                TreeViewItem treeItem = this.CheckBoxParameterCategories.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    this.ExpandAll(treeItem, true);
                }

                treeItem.IsExpanded = true;
            }
        }

        private void ButtonCollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (object item in this.CheckBoxParameterCategories.Items)
            {
                TreeViewItem treeItem = this.CheckBoxParameterCategories.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
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