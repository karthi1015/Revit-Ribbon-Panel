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
    using System.Reflection;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Autodesk.Revit.UI;

    /// <summary>
    /// The app.
    /// </summary>
    public class App : IExternalApplication
    {
        private readonly string addSharedParamsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CreateSharedParams.dll";

        private readonly string addFamilyParametersPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\AddFamilyParameters.dll";

        private const string RibbonTab = "PMTech";

        private const string RibbonPanel = "PMTech Tools";

        public Result OnStartup(UIControlledApplication a)
        {
            // get the ribbon tab
            try
            {
                a.CreateRibbonTab(RibbonTab);
            }
            catch (Exception)
            {
                // ignored
            }

            // Get o create the panel
            RibbonPanel panel = null;
            List<RibbonPanel> panels = a.GetRibbonPanels(RibbonTab);
            foreach (var ribbonPanel in panels)
            {
                if (ribbonPanel.Name == RibbonPanel)
                {
                    panel = ribbonPanel;
                    break;
                }
            }

            // If couldn't find the panel, create it
            if (panel == null)
            {
                panel = a.CreateRibbonPanel(RibbonTab, RibbonPanel);
            }

            // get the image for the button
            Image img = Properties.Resources.icons8_add_to_collection_32;
            ImageSource imageSource = this.GetImageSource(img);

            // create the button data
            PushButtonData btnData = new PushButtonData("Add shared parameters", "Add shared parameters", this.addSharedParamsPath, "CreateSharedParams.CreateSharedParameter")
                                     {
                                         ToolTip = "Batch add shared parameters from excel file", LongDescription = "Batch add shared parameters from excel file", Image = imageSource, LargeImage = imageSource
                                     };

            // add the button to the ribbon
            PushButton button = panel.AddItem(btnData) as PushButton;
            button.Enabled = true;

            // get the image for the button
            Image img2 = Properties.Resources.icons8_add_property_32;
            ImageSource image2Source = this.GetImageSource(img2);

            // create the button data
            PushButtonData btn2Data = new PushButtonData("Add family parameters", "Add family parameters", this.addFamilyParametersPath, "AddFamilyParameters.Command")
                                      {
                                          ToolTip = "Batch add family parameters from excel file", LongDescription = "Batch add family parameters from excel file", Image = image2Source, LargeImage = image2Source
                                      };

            // add the button to the ribbon
            PushButton button2 = panel.AddItem(btn2Data) as PushButton;
            button2.Enabled = true;

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