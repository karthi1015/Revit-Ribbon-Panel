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

            List<RevitParameter> dataList = ParamsHelper.LoadExcel2();

            if (dataList == null)
            {
                throw new ArgumentException("Не удалось получить данные из файла excel");
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

        private static void AddDocumentParameters(List<RevitParameter> dataList, DefinitionFile sharedParameterFile, AddFamilyParametersResult results)
        {
            List<ProjectParameterData> projectParametersData = GetProjectParameterData(revitDocument);
            foreach (RevitParameter parameter in dataList)
            {
                DefinitionGroup dg = ParamsHelper.GetOrCreateSharedParamsGroup(sharedParameterFile, parameter.GroupName);
                ExternalDefinition externalDefinition = ParamsHelper.GetOrCreateSharedParamDefinition(dg, parameter.ParamType, parameter.ParamName, parameter.IsVisible);

                Category category = revitDocument.Settings.Categories.get_Item(parameter.Category);

                bool projectHasParameter = false;
                Binding newIb;
                foreach (var data in projectParametersData)
                {
                    var categories = data.Binding.Categories;

                    if (externalDefinition.GUID.ToString().Equals(data.GUID))
                    {
                        if (categories.Contains(category))
                        {
                            projectHasParameter = true;
                            break;
                        }

                        categories.Insert(category);
                        newIb = revitDocument.Application.Create.NewTypeBinding(categories);
                        if (parameter.IsInstance)
                        {
                            newIb = revitDocument.Application.Create.NewInstanceBinding(categories);
                        }

                        revitDocument.ParameterBindings.ReInsert(data.Definition, newIb, parameter.ParamGroup);
                        results.AddProjectParameterNote(parameter, true);
                        projectHasParameter = true;
                        break;
                    }

                    if (data.Name.Equals(parameter.ParamName))
                    {
                        // different GUID
                        if (categories.Contains(category))
                        {
                            projectHasParameter = true;
                            break;
                        }

                        categories.Insert(category);
                        newIb = revitDocument.Application.Create.NewTypeBinding(categories);
                        if (parameter.IsInstance)
                        {
                            newIb = revitDocument.Application.Create.NewInstanceBinding(categories);
                        }

                        revitDocument.ParameterBindings.ReInsert(data.Definition, newIb, parameter.ParamGroup);
                        results.AddProjectParameterNote(parameter, true);
                        projectHasParameter = true;
                        break;
                    }
                }

                if (!projectHasParameter)
                {
                    CategorySet categorySet = revitDocument.Application.Create.NewCategorySet();
                    categorySet.Insert(category);

                    newIb = revitDocument.Application.Create.NewTypeBinding(categorySet);
                    if (parameter.IsInstance)
                    {
                        newIb = revitDocument.Application.Create.NewInstanceBinding(categorySet);
                    }

                    if (!revitDocument.ParameterBindings.Insert(externalDefinition, newIb, parameter.ParamGroup))
                    {
                        revitDocument.ParameterBindings.ReInsert(externalDefinition, newIb, parameter.ParamGroup);
                    }

                    projectParametersData.Add(
                        new ProjectParameterData
                        {
                            Definition = externalDefinition, Name = externalDefinition.Name, Binding = (ElementBinding)newIb, GUID = externalDefinition.GUID.ToString()
                        });
                    results.AddProjectParameterNote(parameter, false);
                }
            }
        }

        private static List<ProjectParameterData> GetProjectParameterData(Document doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (doc.IsFamilyDocument)
            {
                throw new Exception("doc can not be a family document.");
            }

            List<ProjectParameterData> result = new List<ProjectParameterData>();

            DefinitionBindingMapIterator it = doc.ParameterBindings.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                var definition = (InternalDefinition)it.Key;
                var sharedParameterElement = doc.GetElement(definition.Id) as SharedParameterElement;

                if (sharedParameterElement == null)
                {
                    continue;
                }

                var newProjectParameterData = new ProjectParameterData
                                              {
                                                  Definition = it.Key,
                                                  Name = it.Key.Name,
                                                  Binding = it.Current as ElementBinding,
                                                  GUID = sharedParameterElement.GuidValue.ToString()
                                              };

                result.Add(newProjectParameterData);
            }

            return result;
        }

        private class ProjectParameterData
        {
            public Definition Definition { get; set; } = null;

            public ElementBinding Binding { get; set; } = null;

            public string Name { get; set; } = null;

            public string GUID { get; set; }
        }
    }
}