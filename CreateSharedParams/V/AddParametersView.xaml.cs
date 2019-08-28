// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddParametersView.xaml.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Interaction logic for AddParametersView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateParams.V
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using AddFamilyParameters.V;

    using Autodesk.Revit.DB;

    using CreateParams.VM;

    /// <summary>
    /// Interaction logic for AddParametersView
    /// </summary>
    public partial class AddParametersView : Window
    {
        private readonly AddParametersViewModel viewModel;

        private readonly Document revitDocument;
        private FamilyListView familyListView;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParametersView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AddParametersView(Document doc)
        {
            this.InitializeComponent();
            Image img = Properties.Resources.icons8_add_property_32;
            this.Icon = this.GetImageSource(img);
            this.revitDocument = doc;
            this.viewModel = new AddParametersViewModel(doc);
            
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private BitmapImage GetImageSource(Image img)
        {
            BitmapImage bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;

                bmp.EndInit();
            }

            return bmp;
        }

        private void ButtonBrowseSharedParameterFile_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.SetNewSharedParameterFile();
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonCreateSharedParameterFile_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.CreateNewSharedParametersFile();
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonAddSharedParameters_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.CreateProjectSharedParameter();
        }

        private void ButtonAddFamilyParameters_OnClick(object sender, RoutedEventArgs e)
        {
            this.familyListView = new FamilyListView(this.revitDocument)
                                      {
                                          Owner = this
                                      };

            this.familyListView.ShowDialog();
        }
    }
}
