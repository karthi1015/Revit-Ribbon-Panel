using System;
using System.Windows;

namespace FindParameters.V
{
    using System.Collections.ObjectModel;

    using Autodesk.Revit.DB;

    using FindParameters.M;
    using FindParameters.VM;

    public partial class FindParametersView : Window
    {
        public FindParametersView(Document doc)
        {
            this.InitializeComponent();

            FindParametersViewModel findParametersViewModel = new FindParametersViewModel(doc);
            this.Categories = findParametersViewModel.ParameterCategoriesList;
        }

        public ObservableCollection<RevitBuiltInParameterGroup> Categories { get; set; }

        private void ButtonLoadParametersClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonUncheckAllClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
