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

    using AddFamilyParameters.M;
    using AddFamilyParameters.VM;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The family list window.
    /// </summary>
    public partial class FamilyListView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyListView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public FamilyListView(Document doc)
        {
            try
            {
                this.InitializeComponent();

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
                FamilyListViewModel.AddFamilyParameters(this.Families, this.CheckBoxAddShared.IsChecked ?? false);
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Add Family Parameters", exception.Message);
            }
        }
    }
}