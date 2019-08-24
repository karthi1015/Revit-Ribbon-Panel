// ReSharper disable StyleCop.SA1650
// ReSharper disable StyleCop.SA1600

// ReSharper disable StyleCop.SA1309
// ReSharper disable InconsistentNaming

namespace AddFamilyParameters.V
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

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Application = Autodesk.Revit.ApplicationServices.Application;

    /// <summary>
    /// The family list window.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class FamilyListView : Window, IExternalCommand
    {
        private FamilyListViewModel familyListViewModel;

        private UIApplication uiapp;

        private UIDocument uidoc;

        private Application app;

        private Document doc;

        public FamilyListView()
        {
            try
            {
                this.InitializeComponent();
                this.Families = this.familyListViewModel.FamCategoriesList;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Initializing Component", e.Message);
            }
        }

        public ObservableCollection<FamilyCategory> Families { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            this.uiapp = commandData.Application;
            this.uidoc = this.uiapp.ActiveUIDocument;
            this.app = this.uiapp.Application;
            this.doc = this.uidoc.Document;

            this.familyListViewModel = new FamilyListViewModel(this.doc);

            this.ShowDialog();

            return Result.Succeeded;
        }

        private void ButtonLoadParametersClick(object sender, RoutedEventArgs e)
        {
            List<Family> fam = (from familyCategory in this.Families
                                from item in familyCategory.Members
                                where ItemHelper.GetIsChecked(item) == true
                                select item.Family).ToList();

            this.familyListViewModel.AddFamilyParameters(fam);
        }
    }
}