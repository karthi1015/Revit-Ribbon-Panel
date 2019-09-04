using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FindParameters.VM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FindParameters.M;

    public class FindParametersViewModel
    {
        private static List<RevitBuiltInParameterGroup> parameterCategories;

        private static Document revitDocument;

        public FindParametersViewModel(Document doc)
        {
            revitDocument = doc;

            Dictionary<BuiltInParameterGroup, List<Definition>> paramDictionary = FindParameterCategories();
            InitializeParameterCategoryCollection(paramDictionary);
        }

        public List<RevitBuiltInParameterGroup> ParameterCategoriesList => parameterCategories;

        // private static List<BuiltInParameterGroup> FindParameterCategories()
        // {
        // return Enum.GetValues(typeof(BuiltInParameterGroup)).Cast<BuiltInParameterGroup>().ToList();
        // }
        // private static void InitializeParameterCategoryCollection(List<BuiltInParameterGroup> source)
        // {
        // parameterCategories = source.Select(item => new RevitDefinition() { ParamGroup = item }).OrderBy(e => e.ParamName).ToList();
        // }
        private static void InitializeParameterCategoryCollection(Dictionary<BuiltInParameterGroup, List<Definition>> source)
        {
            parameterCategories = new List<RevitBuiltInParameterGroup>();

            foreach (KeyValuePair<BuiltInParameterGroup, List<Definition>> item in source)
            {
                parameterCategories.Add(new RevitBuiltInParameterGroup(item.Value) { Name = LabelUtils.GetLabelFor(item.Key) });
            }
        }

        private static Dictionary<BuiltInParameterGroup, List<Definition>> FindParameterCategories()
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

            return sortedElements.SelectMany(element => element.GetOrderedParameters(), (element, parameter) => new { element, parameter })
                .GroupBy(t => t.parameter.Definition.ParameterGroup, t => t.parameter.Definition)
                .OrderBy(e => LabelUtils.GetLabelFor(e.Key))
                .ToDictionary(e => e.Key, e => e.ToList());
        }
    }
}