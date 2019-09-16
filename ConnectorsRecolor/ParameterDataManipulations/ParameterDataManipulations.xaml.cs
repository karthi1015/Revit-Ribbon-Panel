namespace Gladkoe.ParameterDataManipulations
{
    using System.Windows;

    using Autodesk.Revit.DB;

    /// <summary>
    /// Interaction logic for ParameterDataManipulations
    /// </summary>
    public partial class ParameterDataManipulations : Window
    {
        public ParameterDataManipulations(Document doc)
        {
            this.InitializeComponent();
            RevitDocument = doc;
        }

        public static Document RevitDocument { get; private set; }

        private void FillParametersBtn_OnClick(object sender, RoutedEventArgs e)
        {
            FillParameters.FillParams(RevitDocument);
        }
    }
}