// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedParameter.cs" company="PMtech">
//   PMtech
// </copyright>
// <summary>
//   My customized model
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateSharedParams.Models
{
    /// <summary>
    /// My customized model
    /// </summary>
    public class SharedParameter
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
        public string ParamType { get; set; }

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        public string Visible { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter group.
        /// </summary>
        public string ParamGroup { get; set; }

        /// <summary>
        /// Gets or sets the shared parameter to be of instance or of type.
        /// </summary>
        public string Instance { get; set; }
    }
}