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

    using Revit_Utilities.Utilities;

    public class Helper
    {
        public static void GetElementsParameters(Document doc)
        {
            var sw = Stopwatch.StartNew();
            Dictionary<string, List<Element>> sortedElements = GetFilteredElementsByCategory(doc);

            using (var workbook = new XLWorkbook())
            {
                var ds = GetDataSet(sortedElements);
                workbook.Worksheets.Add(ds);
                workbook.SaveAs("d:\\fileReport.xlsx");
            }

            sw.Stop();

            TaskDialog.Show(
                "Parameter Export",
                $"{sortedElements.Count} categories and a total " + $"of {sortedElements.Values.SelectMany(list => list).Distinct().Count()} elements exported "
                                                                  + $"in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }

        private static DataSet GetDataSet(Dictionary<string, List<Element>> sortedElements)
        {
            var ds = new DataSet();
            foreach (KeyValuePair<string, List<Element>> element in sortedElements)
            {
                ds.Tables.Add(GetTable(element));
            }

            return ds;
        }

        private static DataTable GetTable(KeyValuePair<string, List<Element>> element)
        {
            DataTable table = new DataTable { TableName = element.Key };

            foreach (Element item in element.Value)
            {
                DataRow row = table.NewRow();
                foreach (Parameter parameter in item.Parameters)
                {
                    if (!table.Columns.Contains(parameter.Definition.Name))
                    {
                        table.Columns.Add(parameter.Definition.Name);
                    }

                    row[parameter.Definition.Name] = LabUtils.GetParameterValue(parameter);
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
                if (e.Category != null)
                {
                    // If this category was not yet encountered, add it and create a new container for its elements.
                    if (!sortedElements.ContainsKey(e.Category.Name))
                    {
                        sortedElements.Add(e.Category.Name, new List<Element>());
                    }

                    sortedElements[e.Category.Name].Add(e);
                }
            }

            return sortedElements;
        }
    }
}