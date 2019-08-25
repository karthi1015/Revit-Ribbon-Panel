// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddFamilyParametersResult.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The add family parameters result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.HelperClass
{
    using System.Collections.Generic;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    /// <summary>
    /// The add family parameters result to config dialogs on load.
    /// </summary>
    public class AddFamilyParametersResult
    {
        /// <summary>
        /// List of text note type names and updated flags.
        /// </summary>
        private List<AddParameterResult> textNoteTypeResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFamilyParametersResult"/> class.
        /// </summary>
        /// <param name="f">
        /// The f.
        /// </param>
        public AddFamilyParametersResult(Element f)
        {
            this.FamilyName = f.Name;
            this.textNoteTypeResults = null;
        }

        /// <summary>
        /// Gets or sets the Family element name in the project database.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the family document used to reload the family.
        /// </summary>
        public Document FamilyDocument { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this family skipped, e.g. this family is not editable.
        /// </summary>
        public bool Skipped { get; set; }

        /// <summary>
        /// The show results dialog.
        /// </summary>
        /// <param name="results">
        /// The results.
        /// </param>
        public static void ShowResultsDialog(List<AddFamilyParametersResult> results)
        {
            TaskDialog d = new TaskDialog("Add parameter") { MainInstruction = $"{results.Count} families processed." };

            List<string> familyResults = results.ConvertAll(r => r.ToString());

            familyResults.Sort();

            d.MainContent = string.Join("\r\n", familyResults);

            d.Show();
        }

        /// <summary>
        /// The add family parameter note.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void AddFamilyParameterNote(FamilyParameter parameter)
        {
            if (this.textNoteTypeResults == null)
            {
                this.textNoteTypeResults = new List<AddParameterResult>();
            }

            AddParameterResult r = new AddParameterResult { Name = parameter.Definition.Name };
            this.textNoteTypeResults.Add(r);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            string s = this.FamilyName + ": ";

            if (this.Skipped)
            {
                s += "skipped";
            }
            else
            {
                int numTotal = this.textNoteTypeResults.Count;

                s += $"{numTotal} parameters processed";
            }

            return s;
        }

        private class AddParameterResult
        {
            public string Name { get; set; }
        }
    }
}