using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using FindParameters.M;
using FindParameters.Utilities;

namespace FindParameters.VM
{
    public class FindParametersViewModel
    {
        private static ObservableCollection<RevitBuiltInParameterGroup> parameterCategories;

        private static Document revitDocument;

        private static Dictionary<string, List<Element>> elements;

        public FindParametersViewModel(Document doc)
        {
            revitDocument = doc;
            elements = ElementsExporter.GetFilteredElementsByCategory(doc);

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

        private Dictionary<string, List<Definition>> FindParameterCategories()
        {
            return elements.Values.SelectMany(e => e)
                .SelectMany(element => element.GetOrderedParameters(), (element, parameter) => new { element, parameter })
                .GroupBy(t => t.parameter.Definition.ParameterGroup, t => t.parameter.Definition)
                .OrderBy(e => LabelUtils.GetLabelFor(e.Key))
                .ToDictionary(e => LabelUtils.GetLabelFor(e.Key), e => e.ToList());
        }

        public static void ExportElementParameters(ObservableCollection<RevitBuiltInParameterGroup> families, bool isChecked)
        {
            List<Definition> pickedDefinitions = (from familyCategory in families
                                    from item in familyCategory.Members
                                    where ItemHelper.GetIsChecked(item) == true
                                    select item.Definition).ToList();

            if (pickedDefinitions.Count == 0)
            {
                throw new ArgumentException("Пожалуйста, выберите семейства, в которые вы хотите добавить параметры");
            }

            ElementsExporter.ExportElementParameters(revitDocument, pickedDefinitions, elements);
        }
    }
}