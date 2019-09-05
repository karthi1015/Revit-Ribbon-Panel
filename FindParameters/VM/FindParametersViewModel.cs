using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using FindParameters.M;
using FindParameters.Utilities;

namespace FindParameters.VM
{
    using Autodesk.Revit.DB.Structure;

    public class FindParametersViewModel
    {
        private static ObservableCollection<RevitBuiltInParameterGroup> parameterCategories;

        private static Dictionary<string, List<Element>> elements;

        public FindParametersViewModel(Document doc)
        {
            elements = ElementsExporter.GetFilteredElementsByCategory(doc);

            Dictionary<string, List<Parameter>> paramDictionary = FindParameterCategories();

            InitializeParameterCategoryCollection(paramDictionary);
        }

        public ObservableCollection<RevitBuiltInParameterGroup> ParameterCategoriesList => parameterCategories;

        public static void ExportElementParameters(ObservableCollection<RevitBuiltInParameterGroup> parameterGroups, bool isChecked)
        {
            List<Parameter> pickedDefinitions = (from parameterGroup in parameterGroups
                                                 from parameter in parameterGroup.Members
                                                 where ItemHelper.GetIsChecked(parameter) == true
                                                 select parameter.Parameter).ToList();

            if (pickedDefinitions.Count == 0)
            {
                throw new ArgumentException("Пожалуйста, выберите параметры, которые вы хотите экспортировать");
            }

            ElementsExporter.ExportElementParameters(pickedDefinitions, elements);
        }

        private static void InitializeParameterCategoryCollection(Dictionary<string, List<Parameter>> source)
        {
            parameterCategories = new ObservableCollection<RevitBuiltInParameterGroup>();

            foreach (var item in source)
            {
                parameterCategories.Add(new RevitBuiltInParameterGroup(item.Value) { Name = item.Key });
            }
        }

        private static Dictionary<string, List<Parameter>> FindParameterCategories()
        {
            return elements.Values.SelectMany(e => e)
                .SelectMany(element => element.GetOrderedParameters(), (element, parameter) => new { element, parameter })
                .GroupBy(t => t.parameter.Definition.ParameterGroup, t => t.parameter)
                .OrderBy(e => LabelUtils.GetLabelFor(e.Key))
                .ToDictionary(e => LabelUtils.GetLabelFor(e.Key), e => e.ToList());
        }

        private class ParameterEqualityComparer : IEqualityComparer<Parameter>
        {
            public bool Equals(Parameter x, Parameter y)
            {
                if ((x == null) && (y == null))
                {
                    return true;
                }

                if ((x == null) || (y == null))
                {
                    return false;
                }

                return x.Definition.Name == y.Definition.Name;
            }

            public int GetHashCode(Parameter obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}