using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public List<RevitBuiltInParameterGroup> Categories { get; set; }

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
