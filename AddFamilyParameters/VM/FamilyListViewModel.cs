namespace AddFamilyParameters.VM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using AddFamilyParameters.M;
    using AddFamilyParameters.Utilities;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The family list view model.
    /// </summary>
    public class FamilyListViewModel
    {
        private static ObservableCollection<FamilyCategory> famCategories;

        private static Document revitDocument;

        public FamilyListViewModel(Document doc)
        {
            revitDocument = doc;
            Dictionary<string, List<Family>> famDictionary = FindFamilyTypes();

            InitializeFamilyCategoryCollection(famDictionary);
        }

        /// <summary>
        /// The family categories list.
        /// </summary>
        public ObservableCollection<FamilyCategory> FamCategoriesList => famCategories;

        private static DefinitionFile SharedParameterFile
        {
            get
            {
                DefinitionFile file; 
                try
                {
                    file = revitDocument.Application.OpenSharedParameterFile();
                }
                catch
                {
                    throw new ArgumentException("Проблема в файле общих параметров. Пожалуйста, выберите другой файл или создайте новый");
                }

                return file;
            }
        }

        /// <summary>
        /// The add family parameters.
        /// </summary>
        /// <param name="families">
        /// The families.
        /// </param>
        /// <param name="isAddShared"></param>
        /// <param name="checkProject"></param>
        /// <exception cref="ArgumentException">Throws when no families are checked
        /// </exception>
        public static void AddFamilyParameters(ObservableCollection<FamilyCategory> families, bool isAddShared, bool checkProject)
        {
            List<Family> fam = (from familyCategory in families
                                from item in familyCategory.Members
                                where ItemHelper.GetIsChecked(item) == true
                                select item.Family).ToList();

            if (fam.Count == 0)
            {
                throw new ArgumentException("Пожалуйста, выберите семейства, в которые вы хотите добавить параметры");
            }

            if (isAddShared && ((SharedParameterFile == null) || (SharedParameterFile.Filename == string.Empty)))
            {
                throw new ArgumentException("Выбранный файл общих параметров не существует. Пожалуйста, выберите другой файл или создайте новый");
            }

            var results = new List<AddFamilyParametersResult>();
            List<RevitParameter> dataList = ParamsHelper.LoadExcel();

            if (dataList != null)
            {
                foreach (Family family in fam)
                {
                    var familyParametersResult = new AddFamilyParametersResult(family);

                    Document familyDoc;

                    if (family.IsEditable)
                    {
                        familyParametersResult.FamilyDocument = familyDoc = revitDocument.EditFamily(family);
                    }
                    else
                    {
                        familyParametersResult.Skipped = true;
                        results.Add(familyParametersResult);
                        continue;
                    }

                    using (var t = new Transaction(familyDoc))
                    {
                        t.Start($"editing {family.Name}");

                        try
                        {
                            AddFamilyParameters(familyDoc, dataList, familyParametersResult, isAddShared, checkProject);
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("Add Family Parameters", e.Message);
                            break;
                        }

                        t.Commit();
                    }

                    results.Add(familyParametersResult);
                }

                if (results.Count != 0)
                {
                    var opt = new FamilyLoadingOptions();

                    foreach (var r in results)
                    {
                        if (r.FamilyDocument != null)
                        {
                            r.FamilyDocument.LoadFamily(revitDocument, opt);
                            r.FamilyDocument.Close(false);
                        }
                    }

                    AddFamilyParametersResult.ShowResultsDialog(results);
                }
            }
        }

        public static void AddFamilyParameters(Document familyDoc, List<RevitParameter> dataList, AddFamilyParametersResult results, bool isAddShared, bool checkProject)
        {
            foreach (var item in dataList)
            {
                bool nameIsInUseInFamilyManager = familyDoc.FamilyManager.Parameters.Cast<FamilyParameter>()
                    .Any(parameter => parameter.Definition.Name == item.ParamName);

                bool nameIsInProject = false;

                if (checkProject)
                {
                    var iter = revitDocument.ParameterBindings.ForwardIterator();

                    while (iter.MoveNext())
                    {
                        if (iter.Key.Name == item.ParamName)
                        {
                            nameIsInProject = true;
                            break;
                        }
                    }
                }

                bool badCategory = item.Category != (BuiltInCategory)familyDoc.OwnerFamily.FamilyCategory.Id.IntegerValue;

                if (item.Category != (BuiltInCategory)familyDoc.OwnerFamily.FamilyCategory.Id.IntegerValue)
                {
                    throw new ArgumentException("В файле Excel параметру назначена неверная категория семейства. Проверьте корректность файла Excel");
                }

                if (nameIsInUseInFamilyManager || badCategory || nameIsInProject)
                {
                    continue;
                }

                if (isAddShared)
                {
                    if ((familyDoc.Application.OpenSharedParameterFile() == null) || (familyDoc.Application.OpenSharedParameterFile().Filename == string.Empty))
                    {
                        throw new ArgumentException("Выбранный файл общих параметров не существует. Пожалуйста, выберите другой файл или создайте новый");
                    }

                    DefinitionGroup dg = ParamsHelper.GetOrCreateSharedParamsGroup(familyDoc.Application.OpenSharedParameterFile(), item.GroupName);
                    ExternalDefinition externalDefinition = ParamsHelper.GetOrCreateSharedParamDefinition(dg, item.ParamType, item.ParamName, item.IsVisible);

                    results.AddFamilyParameterNote(familyDoc.FamilyManager.AddParameter(externalDefinition, item.ParamGroup, item.IsInstance));
                }
                else
                {
                    results.AddFamilyParameterNote(familyDoc.FamilyManager.AddParameter(item.ParamName, item.ParamGroup, item.ParamType, item.IsInstance));
                }
            }
        }

        private static Dictionary<string, List<Family>> FindFamilyTypes()
        {
            return new FilteredElementCollector(revitDocument).WherePasses(new ElementClassFilter(typeof(Family)))
                .Cast<Family>()
                .GroupBy(e => e.FamilyCategory.Name)
                .OrderBy(e => e.Key)
                .ToDictionary(e => e.Key, e => e.ToList());
        }

        private static void InitializeFamilyCategoryCollection(Dictionary<string, List<Family>> source)
        {
            famCategories = new ObservableCollection<FamilyCategory>();
            foreach (KeyValuePair<string, List<Family>> item in source)
            {
                famCategories.Add(new FamilyCategory(item.Value) { Name = item.Key });
            }
        }
    }
}