namespace Revit_Utilities.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
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
        /// The write file.
        /// </summary>
        /// <param name="col">
        /// The col.
        /// </param>
        public static void WriteFile(IEnumerable<Element> col)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Element e in col)
            {
                foreach (Parameter p in e.Parameters)
                {
                    if (p.Definition.Name.Equals("Имя семейства"))
                    {
                        sb.AppendLine(p.AsString());
                    }
                }
            }

            SaveFile(sb);
        }

        /// <summary>
        /// The save file.
        /// </summary>
        /// <param name="sb">
        /// The string builder.
        /// </param>
        public static void SaveFile(StringBuilder sb)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
                                            {
                                                Filter = @"Text Files|*.txt", FilterIndex = 1, RestoreDirectory = true, Title = @"Создать файл общих параметров"
                                            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                using (StreamWriter stream = new StreamWriter(fileName))
                {
                    stream.WriteLine(sb.ToString());
                }
            }
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