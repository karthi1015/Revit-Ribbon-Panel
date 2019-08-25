// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFamily.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The my family.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.M
{
    using System.Windows;

    using Autodesk.Revit.DB;

    /// <summary>
    /// The my family.
    /// </summary>
    public class MyFamily : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyFamily"/> class.
        /// </summary>
        /// <param name="family">
        /// The family.
        /// </param>
        public MyFamily(Family family)
        {
            this.Family = family;
        }

        /// <summary>
        /// Gets the family.
        /// </summary>
        public Family Family { get; private set; }
    }
}