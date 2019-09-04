using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using FindParameters.M;

namespace FindParameters.VM
{
    public class FindParametersViewModel
    {
        private static ObservableCollection<RevitBuiltInParameterGroup> parameterCategories;

        private static Document revitDocument;

        public FindParametersViewModel(Document doc)
        {
            revitDocument = doc;
            Dictionary<string, List<Definition>> paramDictionary = FindParameterCategories();

            InitializeParameterCategoryCollection(paramDictionary);
        }

        public ObservableCollection<RevitBuiltInParameterGroup> ParameterCategoriesList => parameterCategories;

        private static void InitializeParameterCategoryCollection(Dictionary<string, List<Definition>> source)
        {
            parameterCategories = new ObservableCollection<RevitBuiltInParameterGroup>();

            foreach (var item in source)
            {
                parameterCategories.Add(new RevitBuiltInParameterGroup(item.Value) { Name = item.Key });
            }
        }

        private static Dictionary<string, List<Definition>> FindParameterCategories()
        {
            var sortedElements = new List<Element>();

            var els = new FilteredElementCollector(revitDocument).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .Where(e => (e.Category != null) && e.Category.HasMaterialQuantities);

            foreach (Element e in els)
            {
                Parameter volume = e.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED); // Check if element isVoid or Solid. Void elements have volume=0
                if ((e is FamilyInstance fs && (fs.SuperComponent != null)) || ((volume != null) && !volume.HasValue))
                {
                    continue;
                }

                if (!sortedElements.Contains(e))
                {
                    sortedElements.Add(e);
                }
            }

            return sortedElements
                .SelectMany(element => element.GetOrderedParameters(), (element, parameter) => new { element, parameter })
                .GroupBy(t => t.parameter.Definition.ParameterGroup, t => t.parameter.Definition)
                .OrderBy(e => LabelUtils.GetLabelFor(e.Key))
                .ToDictionary(e => LabelUtils.GetLabelFor(e.Key), e => e.ToList());
        }
    }
}