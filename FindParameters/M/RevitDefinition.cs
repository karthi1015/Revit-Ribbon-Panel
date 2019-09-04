namespace FindParameters.M
{
    using System;
    using System.Windows;

    using Autodesk.Revit.DB;

    /// <summary>
    /// The Revit Parameter class that implements dependencyObject
    /// </summary>
    public class RevitDefinition : DependencyObject
    {
        public RevitDefinition(Definition definition)
        {
            this.Definition = definition;
        }

        public Definition Definition { get; set; }
    }
}