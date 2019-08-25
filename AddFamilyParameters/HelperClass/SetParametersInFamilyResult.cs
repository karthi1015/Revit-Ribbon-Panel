// ReSharper disable StyleCop.SA1623
// ReSharper disable StyleCop.SA1600

namespace AddFamilyParameters.HelperClass
{
    using System.Collections.Generic;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    public class SetParametersInFamilyResult
    {
        /// <summary>
        /// List of text note type names and updated flags.
        /// </summary>
        private List<AddParameterResult> textNoteTypeResults;

        public SetParametersInFamilyResult(Element f)
        {
            this.FamilyName = f.Name;
            this.textNoteTypeResults = null;
        }

        /// <summary>
        /// The Family element name in the project database.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The family document used to reload the family.
        /// </summary>
        public Document FamilyDocument { get; set; }

        /// <summary>
        /// Was this family skipped, e.g. this family is not editable.
        /// </summary>
        public bool Skipped { get; set; }

        public bool NeedsReload => this.NumberOfUpdatedTextNoteTypes > 0;

        private int NumberOfUpdatedTextNoteTypes
        {
            get
            {
                return this.textNoteTypeResults?.Count(r => r.Updated) ?? 0;
            }
        }

        public static void ShowResultsDialog(List<SetParametersInFamilyResult> results)
        {
            TaskDialog d = new TaskDialog("Add parameter") { MainInstruction = $"{results.Count} families processed." };

            List<string> familyResults = results.ConvertAll(r => r.ToString());

            familyResults.Sort();

            d.MainContent = string.Join("\r\n", familyResults);

            d.Show();
        }

        public void AddTextNoteType(Family fam, bool updated)
        {
            if (this.textNoteTypeResults == null)
            {
                this.textNoteTypeResults = new List<AddParameterResult>();
            }

            AddParameterResult r = new AddParameterResult { Name = fam.Name, Updated = updated };
            this.textNoteTypeResults.Add(r);
        }

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
                int numUpdated = this.NumberOfUpdatedTextNoteTypes;

                s += $"{numTotal} Families processed, " + $"{numUpdated} updated";
            }

            return s;
        }

        private class AddParameterResult
        {
            public string Name { get; set; }

            public bool Updated { get; set; }
        }
    }
}