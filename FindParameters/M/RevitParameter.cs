namespace FindParameters.M
{
    using System.Windows;

    using Autodesk.Revit.DB;

    /// <summary>
    /// The Revit Parameter class that implements dependencyObject
    /// </summary>
    public class RevitParameter : DependencyObject
    {
        public RevitParameter(Parameter parameter)
        {
            this.Parameter = parameter;
        }

        public Parameter Parameter { get; set; }
    }
}