// ReSharper disable StyleCop.SA1600
namespace AddFamilyParameters.VM
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using AddFamilyParameters.M;

    using Autodesk.Revit.DB;

    public class Service
    {
        private static ObservableCollection<FamilyCategory> famCategories;

        private static Document revitDocument;

        public Service(Document doc)
        {
            revitDocument = doc;

            Dictionary<string, List<Family>> famDictionary = FindFamilyTypes();

            InitializeFamilyCategoryCollection(famDictionary);
        }

        public ObservableCollection<FamilyCategory> FamCategoriesList => famCategories;

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