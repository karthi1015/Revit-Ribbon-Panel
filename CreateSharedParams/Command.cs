// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Command.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   The command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateParams
{
    using System.Windows.Interop;

    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateParams.V;

    /// <summary>
    /// The command.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="commandData">
        /// The command data.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="elements">
        /// The elements.
        /// </param>
        /// <returns>
        /// The <see cref="Result"/>.
        /// </returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            AddParametersView addParametersView = new AddParametersView(doc);

            // makes the WPF form a child of the Revit application
            WindowInteropHelper helper =
                new WindowInteropHelper(addParametersView) { Owner = Autodesk.Windows.ComponentManager.ApplicationWindow };

            addParametersView.ShowDialog();

            return Result.Succeeded;
        }
    }
}