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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    using AddFamilyParameters.HelperClass;
    using AddFamilyParameters.M;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateSharedParams.HelperClass;
    using CreateSharedParams.Models;

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
        /// The add family parameters.
        /// </summary>
        /// <param name="families">
        /// Collection of family categories
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        public static List<SetParametersInFamilyResult> AddFamilyParameters(
            ObservableCollection<FamilyCategory> families)
        {
            List<Family> fam = (from familyCategory in families
                                from item in familyCategory.Members
                                where ItemHelper.GetIsChecked(item) == true
                                select item.Family).ToList();

            List<SetParametersInFamilyResult> results = new List<SetParametersInFamilyResult>();

            foreach (Family family in fam)
            {
                var r1 = new SetParametersInFamilyResult(family);

                Document familyDoc;

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

                    List<SharedParameter> dataList = HelperParams.LoadExcel();

                    foreach (var item in dataList)
                    {
                        var parameterType = (ParameterType)Enum.Parse(typeof(ParameterType), item.ParamType);
                        var parameterGroup = (BuiltInParameterGroup)Enum.Parse(typeof(BuiltInParameterGroup), item.ParamGroup);
                        bool isInstance = bool.Parse(item.Instance);
                        string parameterName = item.ParamName;

                        bool nameIsInUse = familyDoc.FamilyManager.Parameters.Cast<FamilyParameter>()
                                                    .Any(parameter => parameter.Definition.Name == parameterName);

                        if (!nameIsInUse)
                        {
                            familyDoc.FamilyManager.AddParameter(parameterName, parameterGroup, parameterType, isInstance);
                        }
                    }

                    t.Commit();
                }

                r1.AddTextNoteType(family, true);

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

            return results;
        }

        private static void AddFamilyParameters(Document familyDoc)
        {
            List<SharedParameter> dataList = HelperParams.LoadExcel();

            foreach (var item in dataList)
            {
                var parameterType = (ParameterType)Enum.Parse(typeof(ParameterType), item.ParamType);
                var parameterGroup = (BuiltInParameterGroup)Enum.Parse(typeof(BuiltInParameterGroup), item.ParamGroup);
                bool isInstance = bool.Parse(item.Instance);
                string parameterName = item.ParamName;

                bool nameIsInUse = familyDoc.FamilyManager.Parameters.Cast<FamilyParameter>()
                                            .Any(parameter => parameter.Definition.Name != parameterName);

                if (!nameIsInUse)
                {
                    familyDoc.FamilyManager.AddParameter(parameterName, parameterGroup, parameterType, isInstance);
                }
            }
        }

        private static Dictionary<string, List<Family>> FindFamilyTypes()
        {
            return new FilteredElementCollector(revitDocument)
                  .WherePasses(new ElementClassFilter(typeof(Family))).Cast<Family>()
                  .GroupBy(e => e.FamilyCategory.Name).OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.ToList());
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