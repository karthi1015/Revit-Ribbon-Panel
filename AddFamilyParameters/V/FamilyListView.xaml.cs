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
        public FamilyListView(Document doc)
        {
            try
            {
                this.InitializeComponent();

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
            string crew = this.Families.Aggregate(
                string.Empty,
                (current1, family) =>
                    family.Members.Where(person => ItemHelper.GetIsChecked(person) == true)
                          .Aggregate(
                               current1,
                               (current, person) => current + (person.Family.Name + ", ")));

            crew = crew.TrimEnd(',', ' ');
            this.textBoxCrew.Text = "Your crew: " + crew;
        }
    }
}