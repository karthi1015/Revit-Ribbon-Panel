// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FamilyListViewModel.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Defines the FamilyListViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.VM
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    using AddFamilyParameters.HelperClass;
    using AddFamilyParameters.M;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The family list view model.
    /// </summary>
    public class FamilyListViewModel
    {
        private static ObservableCollection<FamilyCategory> famCategories;

        private static Document revitDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyListViewModel"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public FamilyListViewModel(Document doc)
        {
            revitDocument = doc;

            Dictionary<string, List<Family>> famDictionary = FindFamilyTypes();

            InitializeFamilyCategoryCollection(famDictionary);
        }

        /// <summary>
        /// The fam categories list.
        /// </summary>
        public ObservableCollection<FamilyCategory> FamCategoriesList => famCategories;

        /// <summary>
        /// The edit family.
        /// </summary>
        /// <param name="fam">
        /// The fam.
        /// </param>
        public void AddFamilyParameters(List<Family> fam)
        {
            BuiltInParameterGroup addToGroup = BuiltInParameterGroup.INVALID;
            ParameterType parameterType = ParameterType.Text;
            List<SetParametersInFamilyResult> results = new List<SetParametersInFamilyResult>();

            foreach (Family family in fam)
            {
                var r1 = new SetParametersInFamilyResult(family);

                Document familyDoc;
                var updatedParameter = false;
                if (family.IsEditable)
                {
                    r1.FamilyDocument = familyDoc = revitDocument.EditFamily(family);
                }
                else
                {
                    r1.Skipped = true;
                    results.Add(r1);
                    Debug.Print("Family '{0}' is not editable", family.Name);
                    continue;
                }

                using (Transaction t = new Transaction(familyDoc))
                {
                    t.Start($"editing {family.Name}");
                    familyDoc.FamilyManager.AddParameter("MyParameter Name", addToGroup, parameterType, true);
                    t.Commit();
                }

                updatedParameter = true;
                r1.AddTextNoteType(family, updatedParameter);

                results.Add(r1);
            }

            FamilyLoadOptionsMod opt = new FamilyLoadOptionsMod();

            foreach (var r in results)
            {
                if (r.NeedsReload)
                {
                    r.FamilyDocument.LoadFamily(revitDocument, opt);
                    r.FamilyDocument.Close(false);
                }
            }

            TaskDialog d =
                new TaskDialog("Add parameter") { MainInstruction = $"{results.Count} families processed." };

            List<string> familyResults = results.ConvertAll<string>(r => r.ToString());

            familyResults.Sort();

            d.MainContent = string.Join("\r\n", familyResults);

            d.Show();
        }

        private static Dictionary<string, List<Family>> FindFamilyTypes()
        {
            return new FilteredElementCollector(revitDocument)
                  .WherePasses(new ElementClassFilter(typeof(Family))).Cast<Family>()
                  .GroupBy(e => e.FamilyCategory.Name).ToDictionary(e => e.Key, e => e.ToList());
        }

        private static Dictionary<string, List<FamilySymbol>> FindFamilySymbolTypes(BuiltInCategory cat)
        {
            return new FilteredElementCollector(revitDocument)
                  .WherePasses(new ElementClassFilter(typeof(FamilySymbol))).WherePasses(new ElementCategoryFilter(cat))
                  .Cast<FamilySymbol>().GroupBy(e => e.Family.Name).ToDictionary(e => e.Key, e => e.ToList());
        }

        private static void InitializeFamilyCategoryCollection(Dictionary<string, List<Family>> source)
        {
            famCategories = new ObservableCollection<FamilyCategory>();
            foreach (var item in source)
            {
                famCategories.Add(new FamilyCategory(item.Value) { Name = item.Key });
            }
        }
    }
}