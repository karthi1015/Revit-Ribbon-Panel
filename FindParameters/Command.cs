namespace FindParameters
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using FindParameters.Utilities;
    using FindParameters.V;

    using Application = Autodesk.Revit.ApplicationServices.Application;

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // ElementsExporter.GetElementsParameters(doc);
            FindParametersView findParametersView = new FindParametersView(doc);
            findParametersView.ShowDialog();

            return Result.Succeeded;
        }
    }
}