// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParamsHelper.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The parameters helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using AddFamilyParameters.M;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using ClosedXML.Excel;

    /// <summary>
    /// The parameters helper.
    /// </summary>
    public class ParamsHelper
    {
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
        /// <param name="dg">
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
        public static ExternalDefinition GetOrCreateSharedParamDefinition(DefinitionGroup dg, ParameterType parType, string parName, bool visible)
        {
            try
            {
                ExternalDefinition v = (from ExternalDefinition d in dg.Definitions where d.Name == parName select d).ToList().FirstOrDefault();
                if (v == null)
                {
                    var options = new ExternalDefinitionCreationOptions(parName, parType);
                    v = dg.Definitions.Create(options) as ExternalDefinition;
                }

                return v;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR: Failed to get or create Shared Params Definition: {ex.Message}");
                return null;
            }
        }

        public static List<RevitParameter> LoadExcel2()
        {
            List<RevitParameter> myRows = null;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = "Загрузка файла Excel с данными о параметрах"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                myRows = new List<RevitParameter>();
                string excelFilePath = openFileDialog.FileName;

                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(2);

                    foreach (var row in rows)
                    {
                        var paramType = row.Cell(4).CachedValue.ToString();
                        string category = row.Cell(8).CachedValue.ToString();
                        string paramGroup = row.Cell(10).CachedValue.ToString();

                        var myRow = new RevitParameter
                        {
                            ParamName = row.Cell(1).CachedValue.ToString(),
                            GroupName = row.Cell(2).CachedValue.ToString(),
                            ParamType = (ParameterType)Enum.Parse(typeof(ParameterType), paramType),
                            IsVisible = bool.Parse(row.Cell(6).CachedValue.ToString()),
                            Category = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category),
                            ParamGroup = (BuiltInParameterGroup)Enum.Parse(typeof(BuiltInParameterGroup), paramGroup),
                            IsInstance = bool.Parse(row.Cell(12).CachedValue.ToString())
                        };

                        myRows.Add(myRow);
                    }
                }
            }
            return myRows;
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

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = "Загрузка файла Excel с данными о параметрах"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                myRows = new List<RevitParameter>();
                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                string excelFilePath = openFileDialog.FileName;

                // open the excel
                var excelWorkBook = excelApp.Workbooks.Open(excelFilePath);

                // get the first sheet of the excel
                var excelWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)excelWorkBook.Worksheets.Item[1];
                var range = excelWorkSheet.UsedRange;
                int rowCount = range.Rows.Count;

                if (range.Cells.Count < 12)
                {
                    throw new ArgumentException("Была попытка загрузить файл с данными в неверном формате, пожалуйста, попытайтесь еще раз");
                }

                // traverse all the row in the excel
                for (int rowCnt = 3; rowCnt <= rowCount; rowCnt++)
                {
                    try
                    {
                        string paramType = (string)(range.Cells[rowCnt, 4] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString();
                        string category = (string)(range.Cells[rowCnt, 8] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString();
                        string paramGroup = (string)(range.Cells[rowCnt, 10] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString();
                        var myRow = new RevitParameter
                        {
                            ParamName = (string)(range.Cells[rowCnt, 1] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString(),
                            GroupName = (string)(range.Cells[rowCnt, 2] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString(),
                            ParamType =
                                            (ParameterType)Enum.Parse(
                                                typeof(ParameterType),
                                                paramType ?? throw new InvalidOperationException("Невозможно прочитать данный файл, неверный формат данных")),
                            IsVisible =
                                            bool.Parse(
                                                (string)(range.Cells[rowCnt, 6] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString()
                                                ?? throw new InvalidOperationException()),
                            Category = (BuiltInCategory)Enum.Parse(
                                            typeof(BuiltInCategory),
                                            category ?? throw new InvalidOperationException("Невозможно прочитать данный файл, неверный формат данных")),
                            ParamGroup = (BuiltInParameterGroup)Enum.Parse(
                                            typeof(BuiltInParameterGroup),
                                            paramGroup ?? throw new InvalidOperationException("Невозможно прочитать данный файл, неверный формат данных")),
                            IsInstance = bool.Parse(
                                            (string)(range.Cells[rowCnt, 12] as Microsoft.Office.Interop.Excel.Range)?.Value2.ToString()
                                            ?? throw new InvalidOperationException("Невозможно прочитать данный файл, неверный формат данных"))
                        };

                        myRows.Add(myRow);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Read excel", ex.Message);
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