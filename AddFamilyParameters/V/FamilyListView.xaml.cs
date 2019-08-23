// ReSharper disable StyleCop.SA1650
// ReSharper disable StyleCop.SA1600

namespace AddFamilyParameters.V
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows;

    using AddFamilyParameters.M;
    using AddFamilyParameters.VM;

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The family list window.
    /// </summary>
    public partial class FamilyListView : Window
    {
        private Document revitDocument;

        public FamilyListView(Document doc)
        {
            try
            {
                this.InitializeComponent();
                this.revitDocument = doc;
                Service service = new Service(doc);

                // this.ViewsTreeViewFamilies.ItemsSource = service.FamCategoriesList;
                this.Families = service.FamCategoriesList;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Initializing Component", e.Message);
            }
        }

        public ObservableCollection<FamilyCategory> Families { get; set; }

        private void ButtonPrintCrewClick(object sender, RoutedEventArgs e)
        {
            List<Family> fam = (from familyCategory in this.Families
                                from item in familyCategory.Members
                                where ItemHelper.GetIsChecked(item) == true
                                select item.Family).ToList();

            Service.EditFamily(fam);

            this.textBoxCrew.Text = "Success";
        }
    }
}