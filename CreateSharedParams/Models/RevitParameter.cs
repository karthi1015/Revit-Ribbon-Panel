// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevitParameter.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   My customized model
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateSharedParams.Models
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// My customized model
    /// </summary>
    public class RevitParameter
    {
        /// <summary>
        /// Gets or sets the shared parameter name.
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter type.
        /// </summary>
        public ParameterType ParamType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter category.
        /// </summary>
        public BuiltInCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter group.
        /// </summary>
        public BuiltInParameterGroup ParamGroup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether instance.
        /// </summary>
        public bool IsInstance { get; set; }
    }
}