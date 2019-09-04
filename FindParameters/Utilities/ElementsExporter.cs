namespace FindParameters.Utilities
{
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using ClosedXML.Excel;

    using Parameter = Autodesk.Revit.DB.Parameter;

    /// <summary>
    /// Elements export from a Revit document
    /// </summary>
    public class ElementsExporter
    {
        /// <summary>
        /// Exports all elements from a Revit document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="pickedDefinitions"></param>
        /// <param name="sortedElements"></param>
        public static void ExportElementParameters(List<Parameter> pickedDefinitions, Dictionary<string, List<Element>> sortedElements)
        {
            string savingPath = ResultsHelper.GetSaveFilePath();
            if (savingPath == string.Empty)
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            using (var workbook = new XLWorkbook())
            {
                DataSet ds = GetDataSet(sortedElements, pickedDefinitions);
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

        public static Dictionary<string, List<Element>> GetFilteredElementsByCategory(Document doc)
        {
            var sortedElements = new Dictionary<string, List<Element>>();

            var els = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .Where(e => (e.Category != null) && e.Category.HasMaterialQuantities);

            foreach (Element e in els)
            {
                Parameter volume = e.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED); // Check if element isVoid or Solid. Void elements have volume=0
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

        private static DataSet GetDataSet(Dictionary<string, List<Element>> sortedElements, List<Parameter> pickedDefinitions)
        {
            var ds = new DataSet();
            foreach (var element in sortedElements)
            {
                ds.Tables.Add(GetTable(element, pickedDefinitions));
            }

            return ds;
        }

        private static DataTable GetTable(KeyValuePair<string, List<Element>> element, List<Parameter> pickedDefinitions)
        {
            var table = new DataTable { TableName = element.Key };

            table.Columns.Add("ID");
            foreach (Element item in element.Value)
            {
                DataRow row = table.NewRow();
                row["ID"] = item.Id.IntegerValue.ToString();
                foreach (Parameter parameter in item.Parameters)
                {
                    if (pickedDefinitions.Select(p => p.Id).Contains(parameter.Id))
                    {
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

        // TODO if (parameter.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES)
        // TODO select from BuiltInParameterGroup
        // TODO select if you want void elements in table or not
    }
}