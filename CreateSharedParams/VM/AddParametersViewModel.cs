// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddParametersViewModel.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The add parameters view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateParams.VM
{
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    using Autodesk.Revit.DB;

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
                                                    Filter = "Text Files|*.txt",
                                                    FilterIndex = 1,
                                                    RestoreDirectory = true,
                                                    Title = "Задать файл общих параметров"
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
                                                    Filter = "Text Files|*.txt",
                                                    FilterIndex = 1,
                                                    RestoreDirectory = true,
                                                    Title = "Создать файл общих параметров"
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
    }
}