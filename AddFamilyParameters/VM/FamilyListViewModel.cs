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
        /// The families.
        /// </param>
        /// <param name="isAddShared">
        /// The is add shared.
        /// </param>
        /// <exception cref="ArgumentException">Throws when no families are checked
        /// </exception>
        public static void AddFamilyParameters(ObservableCollection<FamilyCategory> families, bool isAddShared)
        {
            List<Family> fam = (from familyCategory in families from item in familyCategory.Members where ItemHelper.GetIsChecked(item) == true select item.Family).ToList();

            if (fam.Count == 0)
            {
                throw new ArgumentException("Please, check families you want to add parameters in");
            }

            List<AddFamilyParametersResult> results = new List<AddFamilyParametersResult>();
            List<RevitParameter> dataList = HelperParams.LoadExcel();

            if (dataList != null)
            {
                foreach (Family family in fam)
                {
                    var r1 = new AddFamilyParametersResult(family);

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

                        try
                        {
                            AddFamilyParameters(familyDoc, dataList, r1, isAddShared);
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("Add Family Parameters", e.Message);
                            break;
                        }

                        t.Commit();
                    }

                    results.Add(r1);
                }

                if (results.Count != 0)
                {
                    FamilyLoadingOptions opt = new FamilyLoadingOptions();

                    foreach (var r in results)
                    {
                        r.FamilyDocument.LoadFamily(revitDocument, opt);
                        r.FamilyDocument.Close(false);
                    }

                    AddFamilyParametersResult.ShowResultsDialog(results);
                }
            }
        }

        private static void AddFamilyParameters(Document familyDoc, List<RevitParameter> dataList, AddFamilyParametersResult results, bool isAddShared)
        {
            DefinitionFile sharedParameterFile = null;

            if (isAddShared)
            {
                sharedParameterFile = HelperParams.GetOrCreateSharedParamsFile(revitDocument, familyDoc.Application);
            }

            foreach (var item in dataList)
            {
                bool nameIsInUse = familyDoc.FamilyManager.Parameters.Cast<FamilyParameter>().Any(parameter => parameter.Definition.Name == item.ParamName);

                if (!nameIsInUse)
                {
                    if (isAddShared)
                    {
                        DefinitionGroup dg = HelperParams.GetOrCreateSharedParamsGroup(sharedParameterFile, item.GroupName);
                        ExternalDefinition externalDefinition = HelperParams.GetOrCreateSharedParamDefinition(dg, item.ParamType, item.ParamName, item.IsVisible);

                        results.AddFamilyParameterNote(familyDoc.FamilyManager.AddParameter(externalDefinition, item.ParamGroup, item.IsInstance));
                    }
                    else
                    {
                        results.AddFamilyParameterNote(familyDoc.FamilyManager.AddParameter(item.ParamName, item.ParamGroup, item.ParamType, item.IsInstance));
                    }
                }
            }
        }

        private static Dictionary<string, List<Family>> FindFamilyTypes()
        {
            return new FilteredElementCollector(revitDocument).WherePasses(new ElementClassFilter(typeof(Family))).Cast<Family>().GroupBy(e => e.FamilyCategory.Name).OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.ToList());
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