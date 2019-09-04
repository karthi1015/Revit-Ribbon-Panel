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

        public static void ExportElementParameters(ObservableCollection<RevitBuiltInParameterGroup> parameterGroups, bool isChecked)
        {
            List<Definition> pickedDefinitions = new List<Definition>();
            foreach (RevitBuiltInParameterGroup parameterGroup in parameterGroups)
            {
                foreach (RevitDefinition definition in parameterGroup.Members)
                {
                    if (ItemHelper.GetIsChecked(definition) == true)
                    {
                        pickedDefinitions.Add(definition.Definition);
                    }
                }
            }

            if (pickedDefinitions.Count == 0)
            {
                throw new ArgumentException("Пожалуйста, выберите семейства, в которые вы хотите добавить параметры");
            }

            ElementsExporter.ExportElementParameters(pickedDefinitions, elements);
        }

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
    }
}