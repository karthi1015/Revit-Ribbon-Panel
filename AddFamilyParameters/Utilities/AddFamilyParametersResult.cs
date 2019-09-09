// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddFamilyParametersResult.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The add family parameters result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using AddFamilyParameters.M;

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

        public AddFamilyParametersResult(Document f)
        {
            this.FamilyName = f.Title;
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
            if (results != null)
            {
                TaskDialog d = new TaskDialog("Add parameter");
                if (results.Count > 1)
                {
                    d = new TaskDialog("Add parameter") { MainInstruction = $"{results.Count} семейств обработано." };
                }

                List<string> familyResults = results.ConvertAll(r => r.ToString());

                familyResults.Sort();

                d.MainContent = string.Join("\r\n", familyResults);

                d.Show();
            }
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

        public void AddFamilyParameterNote(RevitParameter parameter)
        {
            if (this.textNoteTypeResults == null)
            {
                this.textNoteTypeResults = new List<AddParameterResult>();
            }

            AddParameterResult r = new AddParameterResult { Name = parameter.ParamName };
            this.textNoteTypeResults.Add(r);
        }

        public override string ToString()
        {
            string s = string.Empty;
            if (!string.IsNullOrEmpty(FamilyName))
            {
                s = this.FamilyName + ": ";
            }

            if (this.Skipped)
            {
                s += "пропущено";
            }
            else
            {
                int numTotal = 0;
                if (this.textNoteTypeResults != null)
                {
                    numTotal = this.textNoteTypeResults.Count;
                }

                s += $"{numTotal} параметров добавлено.";
            }

            return s;
        }

        private class AddParameterResult
        {
            public string Name { get; set; }
        }
    }
}