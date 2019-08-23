// ReSharper disable StyleCop.SA1600
namespace AddFamilyParameters.M
{
    using System.Windows;

    using Autodesk.Revit.DB;

    public class MyFamily : DependencyObject
    {
        public MyFamily(Family family)
        {
            this.Family = family;
        }

        public Family Family { get; private set; }
    }
}