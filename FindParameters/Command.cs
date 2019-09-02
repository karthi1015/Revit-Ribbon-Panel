namespace FindParameters
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Revit_Utilities.Utilities;

    using Application = Autodesk.Revit.ApplicationServices.Application;
    using Parameter = Autodesk.Revit.DB.Parameter;
    using X = Microsoft.Office.Interop.Excel;

    /// <summary>
    /// The command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            GetElementsParameters(doc);

            return Result.Succeeded;
        }

        private static Dictionary<string, List<Element>> GetFilteredElementsByCategory(Document doc)
        {
            var sortedElements = new Dictionary<string, List<Element>>();

            var els = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
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

        private static void GetElementsParameters(Document doc)
        {
            var sw = Stopwatch.StartNew();

            var sortedElements = GetFilteredElementsByCategory(doc);

            var excel = new X.Application { Visible = true };
            var workbook = excel.Workbooks.Add(Missing.Value);

            bool first = true;
            int numElements = 0;
            int numCategories = sortedElements.Count;

            foreach (var categoryName in sortedElements.Keys)
            {
                List<Element> elementSet = sortedElements[categoryName];

                // Create and name the worksheet
                X.Worksheet worksheet;
                if (first)
                {
                    worksheet = workbook.Sheets.Item[1] as X.Worksheet;

                    first = false;
                }
                else
                {
                    worksheet = excel.Worksheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value) as X.Worksheet;
                }

                sortedElements.TryGetValue(categoryName, out List<Element> el);
                string newName = $"{el.Find(x => x.Category.Name == categoryName).Id.IntegerValue}";
                var name = categoryName.Length > 31 ? newName : categoryName;

                name = name.Replace(':', '_').Replace('/', '_');

                worksheet.Name = name;

                // Determine the names of all parameters 
                // defined for the elements in this set.
                var paramNames = new List<string>();

                foreach (Element e in elementSet)
                {
                    ParameterSet parameters = e.Parameters;

                    foreach (Parameter parameter in parameters)
                    {
                        name = parameter.Definition.Name;

                        if (!paramNames.Contains(name))
                        {
                            paramNames.Add(name);
                        }
                    }
                }

                paramNames.Sort();

                // Add the header row in bold.
                worksheet.Cells[1, 1] = "ID";
                worksheet.Cells[1, 2] = "IsType";

                int column = 3;

                foreach (string paramName in paramNames)
                {
                    worksheet.Cells[1, column] = paramName;
                    ++column;
                }

                var range = worksheet.Range["A1", "Z1"];

                range.Font.Bold = true;
                range.EntireColumn.AutoFit();

                int row = 2;

                foreach (Element e in elementSet)
                {
                    worksheet.Cells[row, 1] = e.Id.IntegerValue;
                    worksheet.Cells[row, 2] = e is ElementType ? 1 : 0;
                    column = 3;

                    foreach (string paramName in paramNames)
                    {
                        var paramValue = "*NA*";

                        Parameter p = e.LookupParameter(paramName);

                        if (p != null)
                        {
                            paramValue = LabUtils.GetParameterValue(p);
                        }

                        worksheet.Cells[row, column++] = paramValue;
                    }

                    ++numElements;
                    ++row;
                }
            }

            sw.Stop();

            TaskDialog.Show("Parameter Export", $"{numCategories} categories and a total " + $"of {numElements} elements exported " + $"in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }
    }
}