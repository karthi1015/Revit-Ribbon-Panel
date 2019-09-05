namespace FindParameters
{
    using System.Windows.Interop;

    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

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

            FindParametersView findParametersView = new FindParametersView(doc);

            WindowInteropHelper helper = new WindowInteropHelper(findParametersView) { Owner = Autodesk.Windows.ComponentManager.ApplicationWindow };

            findParametersView.ShowDialog();

            return Result.Succeeded;
        }
    }
}