// ReSharper disable StyleCop.SA1623
// ReSharper disable StyleCop.SA1600

namespace AddFamilyParameters.M
{
    using System.Collections.Generic;
    using System.Linq;

    using Autodesk.Revit.DB;

    internal class SetParametersInFamilyResult
    {
        /// <summary>
        /// List of text note type names and updated flags.
        /// </summary>
        private List<TextNoteTypeResult> textNoteTypeResults;

        public SetParametersInFamilyResult(Family f)
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

        public void AddTextNoteType(Family fam, bool updated)
        {
            if (this.textNoteTypeResults == null)
            {
                this.textNoteTypeResults = new List<TextNoteTypeResult>();
            }

            TextNoteTypeResult r = new TextNoteTypeResult { Name = fam.Name, Updated = updated };
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

                s += $"{numTotal} parameters processed, " + $"{numUpdated} updated";
            }

            return s;
        }

        private class TextNoteTypeResult
        {
            public string Name { get; set; }

            public bool Updated { get; set; }
        }
    }
}