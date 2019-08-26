// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelperParams.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The helper parameters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateSharedParams.HelperClass
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateSharedParams.Models;

    using AppRvt = Autodesk.Revit.ApplicationServices.Application;
    using BindingRvt = Autodesk.Revit.DB.Binding;
    using Excel = Microsoft.Office.Interop.Excel;

    /// <summary>
    /// The helper parameters.
    /// </summary>
    public static class HelperParams
    {
        /// <summary>
        /// The bind shared parameter result.
        /// </summary>
        public enum BindSharedParamResult
        {
            /// <summary>
            /// already bound.
            /// </summary>
            EAlreadyBound,

            /// <summary>
            /// successfully bound.
            /// </summary>
            ESuccessfullyBound,

            /// <summary>
            /// wrong parameter type.
            /// </summary>
            EWrongParamType,

            /// <summary>
            /// wrong binding type.
            /// </summary>
            EWrongBindingType,

            /// <summary>
            /// binding failed.
            /// </summary>
            EFailed
        }

        /// <summary>
        /// Get Element Parameter *by name*. By default NOT case sensitive. Use overloaded one if case sensitive needed.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="Parameter"/>.
        /// </returns>
        public static Parameter GetElemParam(Element elem, string name)
        {
            return GetElemParam(elem, name, false);
        }

        /// <summary>
        /// Get Element Parameter *by name*. By default NOT case sensitive. Use overloaded one if case sensitive needed.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="matchCase">
        /// The match case.
        /// </param>
        /// <returns>
        /// The <see cref="Parameter"/>.
        /// </returns>
        public static Parameter GetElemParam(Element elem, string name, bool matchCase)
        {
            StringComparison comp = StringComparison.CurrentCultureIgnoreCase;
            if (matchCase)
            {
                comp = StringComparison.CurrentCulture;
            }

            foreach (Parameter p in elem.Parameters)
            {
                if (p.Definition.Name.Equals(name, comp))
                {
                    return p;
                }
            }

            // if here, not found
            return null;
        }

        /// <summary>
        /// The get or create shared parameters file.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="app">
        /// The app.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionFile"/>.
        /// </returns>
        public static DefinitionFile GetOrCreateSharedParamsFile(Document doc, AppRvt app)
        {
            DefinitionFile df = app.OpenSharedParameterFile();

            if (df == null)
            {
                var docFilePath = Path.GetDirectoryName(doc.PathName);

                // Create file if not set yet (ie after Revit installed and no Shared params used so far)
                var fileName = docFilePath + $"\\{Path.GetFileName(doc.PathName)}_SharedParameterFile.txt";

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

                app.SharedParametersFilename = fileName;
                df = app.OpenSharedParameterFile();
            }

            return df;
        }

        /// <summary>
        /// Get or Create Shared Parameters Group
        /// </summary>
        /// <param name="defFile">
        /// The definition file.
        /// </param>
        /// <param name="grpName">
        /// The group name.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionGroup"/>.
        /// </returns>
        public static DefinitionGroup GetOrCreateSharedParamsGroup(DefinitionFile defFile, string grpName)
        {
            try
            {
                DefinitionGroup defGrp = defFile.Groups.get_Item(grpName) ?? defFile.Groups.Create(grpName);
                return defGrp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: Failed to get or create Shared Params Group: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get or Create Shared Parameter Definition
        /// </summary>
        /// <param name="defGrp">
        /// The definition group.
        /// </param>
        /// <param name="parType">
        /// The par type.
        /// </param>
        /// <param name="parName">
        /// The par name.
        /// </param>
        /// <param name="visible">
        /// The visible.
        /// </param>
        /// <returns>
        /// The <see cref="Definition"/>.
        /// </returns>
        public static ExternalDefinition GetOrCreateSharedParamDefinition(DefinitionGroup defGrp, ParameterType parType, string parName, bool visible)
        {
            try
            {
                ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(parName, parType);
                ExternalDefinition def = (defGrp.Definitions.get_Item(parName) ?? defGrp.Definitions.Create(options)) as ExternalDefinition;

                return def;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: Failed to get or create Shared Params Definition: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets or Creates Element's shared parameter.
        /// </summary>
        /// <param name="elem">Revit Element to get parameter for</param>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="grpName">parameter Group Name (relevant only when Creation takes place)</param>
        /// <param name="paramType">parameter Type (relevant only when Creation takes place)</param>
        /// <param name="visible">parameter UI Visibility (relevant only when Creation takes place)</param>
        /// <param name="instanceBinding">parameter Binding: Instance or Type (relevant only when Creation takes place)</param>
        /// <returns>The <see cref="Parameter"/></returns>
        public static Parameter GetOrCreateElemSharedParam(
            Element elem,
            string paramName,
            string grpName,
            ParameterType paramType,
            bool visible,
            bool instanceBinding)
        {
            try
            {
                // Check if existing
                Parameter param = GetElemParam(elem, paramName);
                if (param != null)
                {
                    return param;
                }

                // If here, need to create it...
                BindSharedParamResult res = BindSharedParam(elem.Document, elem.Category, paramName, grpName, paramType, visible, instanceBinding);
                if ((res != BindSharedParamResult.ESuccessfullyBound) && (res != BindSharedParamResult.EAlreadyBound))
                {
                    return null;
                }

                // If here, binding is OK and param seems to be IMMEDIATELY available from the very same command
                return GetElemParam(elem, paramName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in getting or creating Element Param: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// The bind shared Parameter.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="cat">
        /// The cat.
        /// </param>
        /// <param name="paramName">
        /// The Parameter name.
        /// </param>
        /// <param name="grpName">
        /// The group name.
        /// </param>
        /// <param name="paramType">
        /// The Parameter type.
        /// </param>
        /// <param name="visible">
        /// The visible.
        /// </param>
        /// <param name="instanceBinding">
        /// The instance binding.
        /// </param>
        /// <returns>
        /// The <see cref="BindSharedParamResult"/>.
        /// </returns>
        public static BindSharedParamResult BindSharedParam(
            Document doc,
            Category cat,
            string paramName,
            string grpName,
            ParameterType paramType,
            bool visible,
            bool instanceBinding)
        {
            try
            {
                // generic
                AppRvt app = doc.Application;

                // This is needed already here to store old ones for re-inserting
                CategorySet catSet = app.Create.NewCategorySet();

                // Loop all Binding Definitions
                // IMPORTANT NOTE: Categories.Size is ALWAYS 1 !? For multiple categories, there is really one pair per each
                // category, even though the Definitions are the same...
                DefinitionBindingMapIterator iter = doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    Definition def = iter.Key;
                    ElementBinding elemBind = (ElementBinding)iter.Current;

                    // Got param name match
                    if (paramName.Equals(def.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Check for category match - Size is always 1!
                        if ((elemBind != null) && elemBind.Categories.Contains(cat))
                        {
                            // Check Param Type
                            if (paramType != def.ParameterType)
                            {
                                return BindSharedParamResult.EWrongParamType;
                            }

                            // Check Binding Type
                            if (instanceBinding)
                            {
                                if (elemBind.GetType() != typeof(InstanceBinding))
                                {
                                    return BindSharedParamResult.EWrongBindingType;
                                }
                            }
                            else
                            {
                                if (elemBind.GetType() != typeof(TypeBinding))
                                {
                                    return BindSharedParamResult.EWrongBindingType;
                                }
                            }

                            // Check Visibility - cannot (not exposed)
                            // If here, everything is fine, ie already defined correctly
                            return BindSharedParamResult.EAlreadyBound;
                        }

                        // If here, no category match, hence must store "other" cats for re-inserting
                        if (elemBind != null)
                        {
                            foreach (Category catOld in elemBind.Categories)
                            {
                                catSet.Insert(catOld); // 1 only, but no index...
                            }
                        }
                    }
                }

                // If here, there is no Binding Definition for it, so make sure Param defined and then bind it!
                DefinitionFile defFile = GetOrCreateSharedParamsFile(doc, app);
                DefinitionGroup defGrp = GetOrCreateSharedParamsGroup(defFile, grpName);
                Definition definition = GetOrCreateSharedParamDefinition(defGrp, paramType, paramName, visible);
                catSet.Insert(cat);
                BindingRvt bind = null;
                if (instanceBinding)
                {
                    bind = app.Create.NewInstanceBinding(catSet);
                }
                else
                {
                    bind = app.Create.NewTypeBinding(catSet);
                }

                // There is another strange API "feature". If param has EVER been bound in a project (in above iter pairs or even if not there but once deleted), .Insert always fails!? Must use .ReInsert in that case.
                if (doc.ParameterBindings.Insert(definition, bind))
                {
                    return BindSharedParamResult.ESuccessfullyBound;
                }

                return doc.ParameterBindings.ReInsert(definition, bind) ? BindSharedParamResult.ESuccessfullyBound : BindSharedParamResult.EFailed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in Binding Shared Param: {ex.Message}");
                return BindSharedParamResult.EFailed;
            }

            // return BindSharedParamResult.eSuccessfullyBound;
        }

        /// <summary>
        /// The load excel method.
        /// </summary>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        public static List<RevitParameter> LoadExcel()
        {
            List<RevitParameter> myRows = null;

            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xls;*.xlsx", FilterIndex = 1, RestoreDirectory = true };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                myRows = new List<RevitParameter>();
                var excelApp = new Excel.Application();
                var excelFilePath = openFileDialog.FileName;

                // open the excel
                var excelWorkBook = excelApp.Workbooks.Open(excelFilePath);

                // get the first sheet of the excel
                var excelWorkSheet = (Excel.Worksheet)excelWorkBook.Worksheets.Item[1];
                var range = excelWorkSheet.UsedRange;
                int rowCount = range.Rows.Count;

                if (range.Cells.Count < 12)
                {
                    throw new ArgumentException("There was an attempt to load a file with data of the wrong format, please try again");
                }

                // traverse all the row in the excel
                for (int rowCnt = 3; rowCnt <= rowCount; rowCnt++)
                {
                    try
                    {
                        var myRow = new RevitParameter
                        {
                            ParamName = (string)(range.Cells[rowCnt, 1] as Excel.Range)?.Value2.ToString(),
                            GroupName = (string)(range.Cells[rowCnt, 2] as Excel.Range)?.Value2.ToString(),
                            ParamType =
                                            (ParameterType)Enum.Parse(
                                                typeof(ParameterType),
                                                (string)(range.Cells[rowCnt, 4] as Excel.Range)?.Value2.ToString()
                                                ?? throw new InvalidOperationException("Can't read this file, probably wrong data format")),
                            IsVisible =
                                            bool.Parse(
                                                (string)(range.Cells[rowCnt, 6] as Excel.Range)?.Value2.ToString() ?? throw new InvalidOperationException()),
                            Category =
                                            (BuiltInCategory)Enum.Parse(
                                                typeof(BuiltInCategory),
                                                (string)(range.Cells[rowCnt, 8] as Excel.Range)?.Value2.ToString()
                                                ?? throw new InvalidOperationException("Can't read this file, probably wrong data format")),
                            ParamGroup = (BuiltInParameterGroup)Enum.Parse(
                                            typeof(BuiltInParameterGroup),
                                            (string)(range.Cells[rowCnt, 10] as Excel.Range)?.Value2.ToString()
                                            ?? throw new InvalidOperationException("Can't read this file, probably wrong data format")),
                            IsInstance = bool.Parse(
                                            (string)(range.Cells[rowCnt, 12] as Excel.Range)?.Value2.ToString()
                                            ?? throw new InvalidOperationException("Can't read this file, probably wrong data format"))
                        };

                        myRows.Add(myRow);
                    }
                    catch
                    {
                        TaskDialog.Show("Read excel", "Can't read this file, probably wrong data format");
                        break;
                    }
                }

                // release the resources
                excelWorkBook.Close(true);
                excelApp.Quit();
                Marshal.ReleaseComObject(excelWorkSheet);
                Marshal.ReleaseComObject(excelWorkBook);
                Marshal.ReleaseComObject(excelApp);
            }

            return myRows;
        }
    }
}