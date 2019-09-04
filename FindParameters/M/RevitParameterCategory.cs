namespace FindParameters.M
{
    using System.Collections.Generic;
    using System.Windows;

    using Autodesk.Revit.DB;

    using FindParameters.Interfaces;

    public class RevitParameterCategory : DependencyObject, IParent<RevitParameter>
    {
        public RevitParameterCategory(IEnumerable<Parameter> parameterList)
        {
            this.Members = new List<RevitParameter>();

            foreach (Parameter parameter in parameterList)
            {
                this.Members.Add(new RevitParameter(parameter));
            }
        }

        public List<RevitParameter> Members { get; set; }

        public IEnumerable<RevitParameter> GetChildren()
        {
            return this.Members;
        }
    }
}