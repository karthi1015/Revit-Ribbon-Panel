namespace FindParameters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using ClosedXML.Excel;

    using DocumentFormat.OpenXml.Drawing.Diagrams;

    using FindParameters.Utilities;

    using Parameter = Autodesk.Revit.DB.Parameter;

    /// <summary>
    /// Elements export from a Revit document
    /// </summary>
    public static class ElementsExporter
    {
        /// <summary>
        /// Exports all elements from a Revit document
        /// </summary>
        /// <param name="doc"></param>
        public static void GetElementsParameters(Document doc)
        {
            string savingPath = ResultsHelper.GetSaveFilePath();
            if (savingPath == string.Empty)
            {
                return;
            }

            var sw = Stopwatch.StartNew();
            Dictionary<string, List<Element>> sortedElements = GetFilteredElementsByCategory(doc);

            using (var workbook = new XLWorkbook())
            {
                DataSet ds = GetDataSet(sortedElements);
                workbook.Worksheets.Add(ds);
                workbook.SaveAs(savingPath);
            }

            sw.Stop();

            int totalCategories = sortedElements.Count;
            int totalElements = sortedElements.Values.Sum(list => list.Count);
            TaskDialog.Show(
                "Parameter Export",
                $"{totalCategories} categories and a total " + $"of {totalElements} elements exported " + $"in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }

        private static DataSet GetDataSet(Dictionary<string, List<Element>> sortedElements)
        {
            var ds = new DataSet();
            foreach (var element in sortedElements)
            {
                ds.Tables.Add(GetTable(element));
            }

            return ds;
        }

        private static DataTable GetTable(KeyValuePair<string, List<Element>> element)
        {
            var table = new DataTable { TableName = element.Key };

            table.Columns.Add("ID");
            foreach (Element item in element.Value)
            {
                DataRow row = table.NewRow();
                row["ID"] = item.Id.IntegerValue.ToString();
                foreach (Parameter parameter in item.Parameters)
                {
                    {
                        // if (parameter.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES)
                        if (!table.Columns.Contains(parameter.Definition.Name))
                        {
                            table.Columns.Add(parameter.Definition.Name);
                        }

                        row[parameter.Definition.Name] = LabUtils.GetParameterValue(parameter);
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private static Dictionary<string, List<Element>> GetFilteredElementsByCategory(Document doc)
        {
            var sortedElements = new Dictionary<string, List<Element>>();

            var els = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .Where(e => (e.Category != null) && e.Category.HasMaterialQuantities);

            foreach (Element e in els)
            {
                Parameter volume = e.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                if ((e is FamilyInstance fs && (fs.SuperComponent != null)) || ((volume != null) && !volume.HasValue))
                {
                    continue;
                }

                if (!sortedElements.ContainsKey(e.Category.Name))
                {
                    sortedElements.Add(e.Category.Name, new List<Element>());
                }

                sortedElements[e.Category.Name].Add(e);
            }

            return sortedElements;
        }

        // TODO Check if element isVoid or Solid. Void elements have volume=0
        // TODO if (parameter.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES)
        // TODO select from BuiltInParameterGroup
    }
}