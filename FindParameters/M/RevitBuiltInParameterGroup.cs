namespace FindParameters.M
{
    using System.Collections.Generic;
    using System.Windows;

    using Autodesk.Revit.DB;

    using FindParameters.Interfaces;

    /// <summary>
    /// The family category.
    /// </summary>
    public class RevitBuiltInParameterGroup : DependencyObject, IParent<RevitParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevitBuiltInParameterGroup"/> class.
        /// </summary>
        /// <param name="familyList">
        /// The family list.
        /// </param>
        public RevitBuiltInParameterGroup(IEnumerable<Parameter> familyList)
        {
            this.Members = new List<RevitParameter>();

            foreach (Parameter definition in familyList)
            {
                this.Members.Add(new RevitParameter(definition));
            }
        }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        public List<RevitParameter> Members { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        public IEnumerable<RevitParameter> GetChildren()
        {
            return this.Members;
        }
    }
}