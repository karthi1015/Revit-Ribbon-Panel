namespace FindParameters.M
{
    using System.Collections.Generic;
    using System.Windows;

    using Autodesk.Revit.DB;

    using FindParameters.Interfaces;

    /// <summary>
    /// The family category.
    /// </summary>
    public class RevitBuiltInParameterGroup : DependencyObject, IParent<RevitDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevitBuiltInParameterGroup"/> class.
        /// </summary>
        /// <param name="familyList">
        /// The family list.
        /// </param>
        public RevitBuiltInParameterGroup(IEnumerable<Definition> familyList)
        {
            this.Members = new List<RevitDefinition>();

            foreach (Definition definition in familyList)
            {
                this.Members.Add(new RevitDefinition(definition));
            }
        }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        public List<RevitDefinition> Members { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        public IEnumerable<RevitDefinition> GetChildren()
        {
            return this.Members;
        }
    }
}