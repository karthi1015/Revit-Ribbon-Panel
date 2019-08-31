namespace CreateParams.VM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    using AddFamilyParameters.M;
    using AddFamilyParameters.Utilities;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Application = Autodesk.Revit.ApplicationServices.Application;
    using Binding = Autodesk.Revit.DB.Binding;

    /// <summary>
    /// The add parameters view model.
    /// </summary>
    public class AddParametersViewModel
    {
        private static Document revitDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParametersViewModel"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
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
        public static void SetNewSharedParameterFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = @"Text Files|*.txt",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = @"Задать файл общих параметров"
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
        public static void CreateNewSharedParametersFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = @"Text Files|*.txt",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = @"Создать файл общих параметров"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                StringBuilder sb = new StringBuilder();
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
        /// The create project shared parameters.
        /// </summary>
        public static void CreateProjectSharedParameter()
        {
            Application app = revitDocument.Application;
            CategorySet categorySet = app.Create.NewCategorySet();
            DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();

            if (sharedParameterFile == null || sharedParameterFile.Filename == string.Empty)
            {
                TaskDialog.Show("Adding Project Parameters", "Выбранный файл общих параметров не существует. Пожалуйста, выберите другой файл или создайте новый");
            }
            else
            {
                try
                {
                    List<RevitParameter> dataList = ParamsHelper.LoadExcel();

                    bool showResult = false;

                    if (dataList != null)
                    {
                        using (Transaction t = new Transaction(revitDocument))
                        {
                            t.Start($"Adding Project Parameters from Excel");

                            foreach (var item in dataList)
                            {
                                DefinitionGroup dg = ParamsHelper.GetOrCreateSharedParamsGroup(sharedParameterFile, item.GroupName);
                                ExternalDefinition externalDefinition = ParamsHelper.GetOrCreateSharedParamDefinition(dg, item.ParamType, item.ParamName, item.IsVisible);

                                Category category = revitDocument.Settings.Categories.get_Item(item.Category);
                                categorySet.Insert(category);

                                // parameter binding
                                Binding newIb;
                                if (item.IsInstance)
                                {
                                    newIb = app.Create.NewInstanceBinding(categorySet);
                                }
                                else
                                {
                                    newIb = app.Create.NewTypeBinding(categorySet);
                                }

                                revitDocument.ParameterBindings.Insert(externalDefinition, newIb, item.ParamGroup);

                                if (revitDocument.ParameterBindings.Contains(externalDefinition))
                                {
                                    if (revitDocument.ParameterBindings.ReInsert(externalDefinition, newIb))
                                    {
                                        showResult = true;
                                    }
                                }
                            }

                            t.Commit();
                        }

                        if (showResult)
                        {
                            TaskDialog.Show("Adding Project Parameters", "Параметры были успешно добавлены");
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Adding Project Parameters", e.Message);
                }
            }
        }
    }
}