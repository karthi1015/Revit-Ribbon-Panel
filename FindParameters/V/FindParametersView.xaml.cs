using System;
using System.Windows;

namespace FindParameters.V
{
    using System.Collections.ObjectModel;

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
                FindParametersViewModel.ExportElementParameters(this.ParameterGroups, this.CheckBoxUseVoid.IsChecked ?? false);
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
    }
}