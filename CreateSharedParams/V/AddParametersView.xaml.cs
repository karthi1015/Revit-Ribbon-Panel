// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddParametersView.xaml.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Interaction logic for AddParametersView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateParams.V
{
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
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    using AddFamilyParameters.V;

    using Autodesk.Revit.DB;

    using CreateParams.VM;

    /// <summary>
    /// Interaction logic for AddParametersView
    /// </summary>
    public partial class AddParametersView : Window
    {
        private readonly AddParametersViewModel viewModel;

        private readonly FamilyListView familyListView;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParametersView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AddParametersView(Document doc)
        {
            this.InitializeComponent();
            this.viewModel = new AddParametersViewModel(doc);
            this.familyListView = new FamilyListView(doc);
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonBrowseSharedParameterFile_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.SetNewSharedParameterFile();
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonCreateSharedParameterFile_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.CreateNewSharedParametersFile();
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonAddSharedParameters_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.CreateProjectSharedParameter();
        }

        private void ButtonAddFamilyParameters_OnClick(object sender, RoutedEventArgs e)
        {
            this.familyListView.Owner = this;

            this.familyListView.ShowDialog();
        }
    }
}
