// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FamilyLoadingOptions.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The family loading options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.Utilities
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// The family loading options.
    /// </summary>
    public class FamilyLoadingOptions : IFamilyLoadOptions
    {
        /// <summary>
        /// The on family found.
        /// </summary>
        /// <param name="familyInUse">
        /// The family in use.
        /// </param>
        /// <param name="overwriteParameterValues">
        /// The overwrite parameter values.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        /// <summary>
        /// The on shared family found.
        /// </summary>
        /// <param name="sharedFamily">
        /// The shared family.
        /// </param>
        /// <param name="familyInUse">
        /// The family in use.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="overwriteParameterValues">
        /// The overwrite parameter values.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool OnSharedFamilyFound(
            Family sharedFamily,
            bool familyInUse,
            out FamilySource source,
            out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}