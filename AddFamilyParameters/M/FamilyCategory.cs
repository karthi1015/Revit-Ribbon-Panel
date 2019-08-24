// ReSharper disable StyleCop.SA1600

namespace AddFamilyParameters.M
{
    using System.Collections.Generic;
    using System.Windows;

    using AddFamilyParameters.Interfaces;

    using Autodesk.Revit.DB;

    public class FamilyCategory : DependencyObject, IParent<object>
    {
        public FamilyCategory(IEnumerable<Family> familyList)
        {
            this.Members = new List<MyFamily>();

            foreach (var family in familyList)
            {
                this.Members.Add(new MyFamily(family));
            }
        }

        public List<MyFamily> Members { get; set; }

        public string Name { get; set; }

        IEnumerable<object> IParent<object>.GetChildren()
        {
            return this.Members;
        }
    }
}