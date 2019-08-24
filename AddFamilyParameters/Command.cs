// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSharedParameter.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Defines the CreateSharedParameter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable StyleCop.SA1600
// ReSharper disable StyleCop.SA1402

namespace AddFamilyParameters
{
    using AddFamilyParameters.V;

    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //FamilyListView familyListView = new FamilyListView(doc);
            //familyListView.ShowDialog();


            return Result.Succeeded;
        }
    }
}
