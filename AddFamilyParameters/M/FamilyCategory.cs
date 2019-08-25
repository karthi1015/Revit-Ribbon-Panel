// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FamilyCategory.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The family category.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.M
{
    using System.Collections.Generic;
    using System.Windows;

    using AddFamilyParameters.Interfaces;

    using Autodesk.Revit.DB;

    /// <summary>
    /// The family category.
    /// </summary>
    public class FamilyCategory : DependencyObject, IParent<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyCategory"/> class.
        /// </summary>
        /// <param name="familyList">
        /// The family list.
        /// </param>
        public FamilyCategory(IEnumerable<Family> familyList)
        {
            this.Members = new List<MyFamily>();

            foreach (var family in familyList)
            {
                this.Members.Add(new MyFamily(family));
            }
        }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        public List<MyFamily> Members { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        IEnumerable<object> IParent<object>.GetChildren()
        {
            return this.Members;
        }
    }
}