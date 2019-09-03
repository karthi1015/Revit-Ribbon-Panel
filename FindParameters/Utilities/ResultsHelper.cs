namespace FindParameters.Utilities
{
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The result showing helper.
    /// </summary>
    public class ResultsHelper
    {
        /// <summary>
        /// The save file.
        /// </summary>
        public static string GetSaveFilePath()
        {
            var saveFileDialog = new SaveFileDialog()
                                 {
                                     Filter = @"Excel Files|*.xlsx", FilterIndex = 1, RestoreDirectory = true, Title = @"Сохранить Excel файл"
                                 };

            return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.FileName : string.Empty;
        }

        /// <summary>
        /// The show results.
        /// </summary>
        /// <param name="elements">
        /// The elements.
        /// </param>
        public static void ShowResults(IEnumerable<Element> elements)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Element e in elements)
            {
                sb.AppendLine($"{e.Name}-{e.Id}");
            }

            TaskDialog.Show("Revit", sb.ToString());
        }
    }
}