namespace CreateParams.VM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using AddFamilyParameters.M;
    using AddFamilyParameters.Utilities;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Binding = Autodesk.Revit.DB.Binding;

    /// <summary>
    /// The add parameters view model.
    /// </summary>
    public class AddParametersViewModel
    {
        private static Document revitDocument;

        public AddParametersViewModel(Document doc)
        {
            revitDocument = doc;
        }

        /// <summary>
        /// Gets or sets the shared parameters file path.
        /// </summary>
        public string SharedParametersFilePath => revitDocument.Application.SharedParametersFilename;

        /// <summary>
        /// The set new shared parameter file.
        /// </summary>
        public static void SetNewSharedParameterFile(string initialDir)
        {
            var openFileDialog = new OpenFileDialog()
                                 {
                                     Filter = @"Text Files|*.txt",
                                     FilterIndex = 1,
                                     RestoreDirectory = true,
                                     Title = @"Задать файл общих параметров",
                                     InitialDirectory = initialDir
                                 };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                revitDocument.Application.SharedParametersFilename = filePath;
            }
        }

        /// <summary>
        /// The create new shared parameters file.
        /// </summary>
        public static void CreateNewSharedParametersFile(string initialDir)
        {
            var saveFileDialog = new SaveFileDialog()
                                 {
                                     Filter = @"Text Files|*.txt",
                                     FilterIndex = 1,
                                     RestoreDirectory = true,
                                     Title = @"Создать файл общих параметров",
                                     InitialDirectory = initialDir
                                 };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                var sb = new StringBuilder();
                sb.AppendLine("# This is a Revit shared parameter file.");
                sb.AppendLine("# Do not edit manually.");
                sb.AppendLine("*META	VERSION	MINVERSION");
                sb.AppendLine("META	2	1");
                sb.AppendLine("*GROUP	ID	NAME");
                sb.AppendLine("*PARAM	GUID	NAME	DATATYPE	DATACATEGORY	GROUP	VISIBLE	DESCRIPTION	USERMODIFIABLE");

                using (StreamWriter stream = new StreamWriter(fileName))
                {
                    stream.WriteLine(sb.ToString());
                }

                revitDocument.Application.SharedParametersFilename = fileName;
            }
        }

        /// <summary>
        /// Create project parameters.
        /// </summary>
        public static void CreateProjectParameter(bool isAddShared)
        {
            DefinitionFile sharedParameterFile;
            try
            {
                sharedParameterFile = revitDocument.Application.OpenSharedParameterFile();
            }
            catch
            {
                throw new ArgumentException("Проблема в файле общих параметров. Пожалуйста, выберите другой файл или создайте новый");
            }

            if (isAddShared && ((sharedParameterFile == null) || (sharedParameterFile.Filename == string.Empty)))
            {
                throw new ArgumentException("Выбранный файл общих параметров не существует. Пожалуйста, выберите другой файл или создайте новый");
            }

            try
            {
                List<RevitParameter> dataList = ParamsHelper.LoadExcel();

                if (dataList == null)
                {
                    return;
                }

                AddFamilyParametersResult familyParametersResult;
                using (var t = new Transaction(revitDocument))
                {
                    t.Start($"Adding Parameters from Excel");

                    if (revitDocument.IsFamilyDocument)
                    {
                        familyParametersResult = new AddFamilyParametersResult(revitDocument.OwnerFamily);
                        AddFamilyParameters.VM.FamilyListViewModel.AddFamilyParameters(revitDocument, dataList, familyParametersResult, isAddShared, false);
                    }
                    else
                    {
                        familyParametersResult = new AddFamilyParametersResult(revitDocument);
                        AddDocumentParameters(dataList, sharedParameterFile, familyParametersResult);
                    }

                    t.Commit();
                }

                AddFamilyParametersResult.ShowResultsDialog(new List<AddFamilyParametersResult> { familyParametersResult });
            }
            catch (Exception e)
            {
                TaskDialog.Show("Adding Project Parameters", e.Message);
            }
        }

        // private static void AddDocumentParameters(List<RevitParameter> dataList, DefinitionFile sharedParameterFile, AddFamilyParametersResult results)
        // {
        // CategorySet categorySet = revitDocument.Application.Create.NewCategorySet();
        // foreach (RevitParameter item in dataList)
        // {
        // DefinitionGroup dg = ParamsHelper.GetOrCreateSharedParamsGroup(sharedParameterFile, item.GroupName);
        // ExternalDefinition externalDefinition = ParamsHelper.GetOrCreateSharedParamDefinition(dg, item.ParamType, item.ParamName, item.IsVisible);
        // Category category = revitDocument.Settings.Categories.get_Item(item.Category);
        // categorySet.Insert(category);
        // Binding newIb;
        // if (item.IsInstance)
        // {
        // newIb = revitDocument.Application.Create.NewInstanceBinding(categorySet);
        // }
        // else
        // {
        // newIb = revitDocument.Application.Create.NewTypeBinding(categorySet);
        // }
        // revitDocument.ParameterBindings.Insert(externalDefinition, newIb, item.ParamGroup);
        // if (revitDocument.ParameterBindings.Contains(externalDefinition))
        // {
        // revitDocument.ParameterBindings.ReInsert(externalDefinition, newIb, item.ParamGroup);
        // }
        // results.AddFamilyParameterNote(item);
        // }
        private static void AddDocumentParameters(List<RevitParameter> dataList, DefinitionFile sharedParameterFile, AddFamilyParametersResult results)
        {
            foreach (RevitParameter parameter in dataList)
            {
                DefinitionGroup dg = ParamsHelper.GetOrCreateSharedParamsGroup(sharedParameterFile, parameter.GroupName);

                // ExternalDefinition externalDefinition = ParamsHelper.GetOrCreateSharedParamDefinition(dg, parameter.ParamType, parameter.ParamName, parameter.IsVisible);
                ExternalDefinition externalDefinition = (from ExternalDefinition d in dg.Definitions where d.Name == parameter.ParamName select d).ToList().FirstOrDefault();
                if (externalDefinition == null)
                {
                    var options = new ExternalDefinitionCreationOptions(parameter.ParamName, parameter.ParamType);
                    externalDefinition = dg.Definitions.Create(options) as ExternalDefinition;
                }

                Category category = revitDocument.Settings.Categories.get_Item(parameter.Category);
                CategorySet categorySet = revitDocument.Application.Create.NewCategorySet();
                categorySet.Insert(category);

                Binding newIb = revitDocument.Application.Create.NewTypeBinding(categorySet);
                if (parameter.IsInstance)
                {
                    newIb = revitDocument.Application.Create.NewInstanceBinding(categorySet);
                }

                var iterator = revitDocument.ParameterBindings.ForwardIterator();
                iterator.Reset();
                bool projectHasParameter = false;
                while (iterator.MoveNext())
                {
                    if (iterator.Key.Name == parameter.ParamName)
                    {
                        projectHasParameter = true;
                        break;
                    }
                }

                if (!projectHasParameter && !revitDocument.ParameterBindings.Contains(externalDefinition))
                {
                    if (!revitDocument.ParameterBindings.Insert(externalDefinition, newIb, parameter.ParamGroup))
                    {
                        revitDocument.ParameterBindings.ReInsert(externalDefinition, newIb, parameter.ParamGroup);
                    }
                }

                results.AddFamilyParameterNote(parameter);
            }
        }

        private static void RawCreateProjectParameterFromExistingSharedParameter(
            DefinitionFile sharedParameterFile,
            string name,
            CategorySet cats,
            BuiltInParameterGroup group,
            bool inst)
        {
            var v = (from DefinitionGroup dg in sharedParameterFile.Groups from ExternalDefinition d in dg.Definitions where d.Name == name select d).ToList();
            if ((v == null) || !v.Any())
            {
                throw new Exception("Invalid Name Input!");
            }

            ExternalDefinition def = v.First();

            Binding binding = revitDocument.Application.Create.NewTypeBinding(cats);
            if (inst)
            {
                binding = revitDocument.Application.Create.NewInstanceBinding(cats);
            }

            revitDocument.ParameterBindings.Insert(def, binding, group);
        }

        private static void RawCreateProjectParameterFromNewSharedParameter(
            string defGroup,
            string name,
            ParameterType type,
            bool visible,
            CategorySet cats,
            BuiltInParameterGroup paramGroup,
            bool inst)
        {
            ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(name, type);
            ExternalDefinition def = revitDocument.Application.OpenSharedParameterFile().Groups.Create(defGroup).Definitions.Create(options) as ExternalDefinition;

            Binding binding = revitDocument.Application.Create.NewTypeBinding(cats);
            if (inst)
            {
                binding = revitDocument.Application.Create.NewInstanceBinding(cats);
            }

            revitDocument.ParameterBindings.Insert(def, binding, paramGroup);
        }

        private static void RawCreateProjectParameter(string name, ParameterType type, bool visible, CategorySet cats, BuiltInParameterGroup group, bool inst)
        {
            string oriFile = revitDocument.Application.SharedParametersFilename;
            string tempFile = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFile))
            {
            }

            revitDocument.Application.SharedParametersFilename = tempFile;
            ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(name, type);
            ExternalDefinition def = revitDocument.Application.OpenSharedParameterFile().Groups.Create("TemporaryDefintionGroup").Definitions.Create(options) as ExternalDefinition;

            revitDocument.Application.SharedParametersFilename = oriFile;
            File.Delete(tempFile);

            Binding binding = revitDocument.Application.Create.NewTypeBinding(cats);
            if (inst)
            {
                binding = revitDocument.Application.Create.NewInstanceBinding(cats);
            }

            revitDocument.ParameterBindings.Insert(def, binding, group);
        }
    }
}