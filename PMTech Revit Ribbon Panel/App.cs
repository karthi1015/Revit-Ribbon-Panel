// --------------------------------------------------------------------------------------------------------------------
// <copyright company="PMTech" file="App.cs">
//   PMTech
// </copyright>
// <summary>
//   
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable StyleCop.SA1600

// ReSharper disable StyleCop.SA1203

namespace PMTech_Revit_Ribbon_Panel
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Autodesk.Revit.UI;

    /// <summary>
    /// The app.
    /// </summary>
    public class App : IExternalApplication
    {
        private readonly string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private const string RibbonTab = "PMTech";

        private const string RibbonPanel = "PMTech Tools";

        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                a.CreateRibbonTab(RibbonTab);
            }
            catch (Exception)
            {
                // ignored
            }

            // Get o create the panel
            List<RibbonPanel> panels = a.GetRibbonPanels(RibbonTab);
            RibbonPanel panel = panels.FirstOrDefault(ribbonPanel => ribbonPanel.Name == RibbonPanel) ?? a.CreateRibbonPanel(RibbonTab, RibbonPanel);

            // ----------------------------
            // ---Button Add shared parameters---
            // ----------------------------
            // get the image for the button
            Image img = Properties.Resources.icons8_add_property_32;
            ImageSource imageSource = this.GetImageSource(img);

            // create the button data
            var addSharedBtnData = new PushButtonData("Add shared parameters", "Добавить параметры", this.path + "\\CreateParams.dll", "CreateParams.Command")
                                     {
                                         ToolTip = "Пакетное добавление параметров в проект или в семейства по excel файлу",
                                         LongDescription = "Разработчик: Кожевников Андрей Олегович",
                                         Image = imageSource,
                                         LargeImage = imageSource
                                     };

            // add the button to the ribbon
            if (panel.AddItem(addSharedBtnData) is PushButton addSharedButton)
            {
                addSharedButton.Enabled = true;
            }

            // ----------------------------
            // ---Button Add shared parameters---
            // ----------------------------

            // ----------------------------
            // ---Button get parameters---
            // ----------------------------
            img = Properties.Resources.icons8_add_to_collection_32;
            imageSource = this.GetImageSource(img);

            var exportParametersBtnData = new PushButtonData("Export parameters", "Экспортировать параметры", this.path + "\\FindParameters.dll", "FindParameters.Command")
                                     {
                                         ToolTip = "Экспорт параметров элементов модели в excel",
                                         LongDescription = "Разработчик: Кожевников Андрей Олегович",
                                         Image = imageSource,
                                         LargeImage = imageSource
                                     };

            if (panel.AddItem(exportParametersBtnData) is PushButton exportParametersButton)
            {
                exportParametersButton.Enabled = true;
            }

            // ----------------------------
            // ---Button get parameters---
            // ----------------------------
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
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
    }
}