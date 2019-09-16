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
        public FindParametersViewModel(Document doc)
        {
            Elements = ElementsExporter.GetFilteredElementsByCategory(doc);

            InitializeParameterCategoryCollection(FindParameterCategories());
        }

        public static ObservableCollection<RevitBuiltInParameterGroup> ParameterCategoriesList { get; private set; }

        public static Dictionary<string, List<Element>> Elements { get; private set; }

        public static void ExportElementParameters(ObservableCollection<RevitBuiltInParameterGroup> parameterGroups, bool isUseVoidChecked, bool isUseHidden)
        {
            List<Parameter> pickedDefinitions = (from parameterGroup in parameterGroups
                                                 from parameter in parameterGroup.Members
                                                 where ItemHelper.GetIsChecked(parameter) == true
                                                 select parameter.Parameter).ToList();

            if (pickedDefinitions.Count == 0)
            {
                throw new ArgumentException("Пожалуйста, выберите параметры, которые вы хотите экспортировать");
            }

            ElementsExporter.ExportElementParameters(pickedDefinitions, Elements);
        }

        private static void InitializeParameterCategoryCollection(Dictionary<string, List<Parameter>> source)
        {
            ParameterCategoriesList = new ObservableCollection<RevitBuiltInParameterGroup>();

            foreach (var item in source)
            {
                ParameterCategoriesList.Add(new RevitBuiltInParameterGroup(item.Value) { Name = item.Key });
            }
        }

        private static Dictionary<string, List<Parameter>> FindParameterCategories()
        {
            return Elements.Values.SelectMany(e => e)
                .SelectMany(e => e.GetOrderedParameters())
                .GroupBy(p => p.Definition.Name)
                .Select(p => p.First())
                .OrderBy(p => p.Definition.Name)
                .GroupBy(p => p.Definition.ParameterGroup, p => p)
                .OrderBy(grp => LabelUtils.GetLabelFor(grp.Key))
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

                return x.Definition.Name.Equals(y.Definition.Name);
            }

            public int GetHashCode(Parameter obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}