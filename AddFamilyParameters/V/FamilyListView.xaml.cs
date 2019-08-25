﻿namespace AddFamilyParameters.V
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows;

    using AddFamilyParameters.HelperClass;
    using AddFamilyParameters.M;
    using AddFamilyParameters.VM;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateSharedParams.HelperClass;
    using CreateSharedParams.Models;

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
            List<SetParametersInFamilyResult> results = null;
            var isChecked = this.CheckBoxAddShared.IsChecked;
            if ((isChecked != null) && !(bool)isChecked)
            {
                results = FamilyListViewModel.AddFamilyParameters(this.Families);
                SetParametersInFamilyResult.ShowResultsDialog(results);
            }
        }
    }
}