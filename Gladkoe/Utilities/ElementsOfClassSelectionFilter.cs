namespace Gladkoe.Utilities
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI.Selection;

    internal class ElementsOfClassSelectionFilter<T> : ISelectionFilter
        where T : Element
    {
        public bool AllowElement(Element elem)
        {
            return elem is T;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}