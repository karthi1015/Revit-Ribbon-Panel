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
    using Autodesk.Revit.DB;

    /// <summary>
    /// The add parameters view model.
    /// </summary>
    public class AddParametersViewModel
    {
        private static Document revitDocument;

        private static string sharedParametersFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParametersViewModel"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AddParametersViewModel(Document doc)
        {
            revitDocument = doc;
            GetSharedParametersFilePath();
        }

        /// <summary>
        /// Gets or sets the shared parameters file path.
        /// </summary>
        public string SharedParametersFilePath => sharedParametersFilePath;

        private static void GetSharedParametersFilePath()
        {
            sharedParametersFilePath = revitDocument.Application.SharedParametersFilename;
        }
    }
}