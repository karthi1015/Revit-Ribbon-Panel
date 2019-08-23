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

        public static void EditFamily(List<Family> fam)
        {
            foreach (Family family in fam)
            {
                var familyDoc = revitDocument.EditFamily(family);

                using (Transaction t = new Transaction(familyDoc))
                {
                    t.Start($"editing {family.Name}");
                    BuiltInParameterGroup addToGroup = BuiltInParameterGroup.INVALID;
                    ParameterType parameterType = ParameterType.Text;
                    familyDoc.FamilyManager.AddParameter("MyParameter Name", addToGroup, parameterType, true);
                    t.Commit();
                }

                revitDocument.LoadFamily(familyDoc, new FamilyLoadOptionsMod());
                familyDoc.Close(false);
            }
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

    class FamilyLoadOptionsMod : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(
            Family sharedFamily,
            bool familyInUse,
            out FamilySource source,
            out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}