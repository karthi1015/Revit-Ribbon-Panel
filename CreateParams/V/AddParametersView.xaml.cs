namespace CreateParams.V
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using AddFamilyParameters.V;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateParams.VM;

    using Visibility = System.Windows.Visibility;

    public partial class AddParametersView : Window
    {
        private readonly AddParametersViewModel viewModel;

        private readonly Document revitDocument;
        private FamilyListView familyListView;

        public AddParametersView(Document doc)
        {
            this.InitializeComponent();

            Image img = Properties.Resources.icons8_add_property_32;
            this.Icon = GetImageSource(img);
            this.revitDocument = doc;

            this.viewModel = new AddParametersViewModel(doc);

            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
            this.AddParameterGroupBox.Header = doc.IsFamilyDocument ? "Текущее семейство" : "Текущий проект";
            this.CheckBoxAddShared.Visibility = doc.IsFamilyDocument ? Visibility.Visible : Visibility.Hidden;
            this.CheckBoxAddShared.Margin = doc.IsFamilyDocument ? new Thickness(20, 20, 0, 0) : new Thickness(20, 0, 0, 0);
            this.Height = doc.IsFamilyDocument ? 240 : 220;
            this.MinHeight = doc.IsFamilyDocument ? 240 : 220;
        }

        private static BitmapImage GetImageSource(Image img)
        {
            BitmapImage bmp = new BitmapImage();

            using (var ms = new MemoryStream())
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
            AddParametersViewModel.SetNewSharedParameterFile(Path.GetDirectoryName(this.FilePathTextBox.Text));
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonCreateSharedParameterFile_OnClick(object sender, RoutedEventArgs e)
        {
            AddParametersViewModel.CreateNewSharedParametersFile(Path.GetDirectoryName(this.FilePathTextBox.Text));
            this.FilePathTextBox.Text = this.viewModel.SharedParametersFilePath;
        }

        private void ButtonAddSharedParameters_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddParametersViewModel.CreateProjectParameter(this.CheckBoxAddShared.IsChecked ?? false);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Revit", ex.Message);
            }
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
