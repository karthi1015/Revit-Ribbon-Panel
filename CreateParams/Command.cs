using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using CreateParams.V;

namespace CreateParams
{
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

            AddParametersView addParametersView = new AddParametersView(doc);

            // makes the WPF form a child of the Revit application
            WindowInteropHelper helper = new WindowInteropHelper(addParametersView) { Owner = Autodesk.Windows.ComponentManager.ApplicationWindow };

            addParametersView.ShowDialog();

            return Result.Succeeded;
        }
    }
}