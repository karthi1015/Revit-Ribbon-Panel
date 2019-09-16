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

    public class App : IExternalApplication
    {
        private const string RibbonTab = "PMTech";

        private const string RibbonPanel = "PMTech Tools";

        private readonly string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            ImageSource imageSource = GetImageSource(img);

            // create the button data
            var addSharedBtnData = new PushButtonData("Add shared parameters", "Добавить параметры", this.path + "\\CreateParams.dll", "CreateParams.Command")
                                   {
                                       ToolTip = "Пакетное добавление параметров в проект или в семейства по excel файлу",
                                       LongDescription = "Разработчик: Кожевников Андрей",
                                       Image = imageSource,
                                       LargeImage = imageSource
                                   };

            // add the button to the ribbon
            if (panel.AddItem(addSharedBtnData) is PushButton addSharedButton)
            {
                addSharedButton.Enabled = true;
            }

            panel.AddSeparator();

            // ----------------------------
            // ---Button get parameters---
            // ----------------------------
            img = Properties.Resources.icons8_export_csv_32;
            imageSource = GetImageSource(img);

            var exportParametersBtnData = new PushButtonData("Export model", "Выгрузить модель", this.path + "\\FindParameters.dll", "FindParameters.Command")
                                          {
                                              ToolTip = "Выгрузить модель в excel",
                                              LongDescription = "Разработчик: Кожевников Андрей",
                                              Image = imageSource,
                                              LargeImage = imageSource
                                          };

            if (panel.AddItem(exportParametersBtnData) is PushButton exportParametersButton)
            {
                exportParametersButton.Enabled = true;
            }

            panel.AddSeparator();

            // ----------------------------
            // ---Button Gladkoe---
            // ----------------------------
            img = Properties.Resources.icons8_administrative_tools_32;
            imageSource = GetImageSource(img);

            PulldownButtonData pullDownDataGladkoe = new PulldownButtonData("Gladkoe", "НБ_Гладкое")
                                                     {
                                                         Image = imageSource, LargeImage = imageSource, ToolTip = "Утилиты для проекта НБ_Гладкое"
                                                     };
            PulldownButton pullDownButtonGladkoe = panel.AddItem(pullDownDataGladkoe) as PulldownButton;

            img = Properties.Resources.icons8_paint_palette_32;
            imageSource = GetImageSource(img);

            var colorConnectorsBtnData = new PushButtonData("Color Connectors", "Окраска коннекторов", this.path + "\\Gladkoe.dll", "Gladkoe.ConnectorsRecolor")
                                         {
                                             ToolTip = "Окраска коннекторов в проекте НБ_Гладкое",
                                             LongDescription = "Разработчик: Кожевников Андрей,\n Климович Александр",
                                             Image = imageSource,
                                             LargeImage = imageSource
                                         };

            pullDownButtonGladkoe.AddPushButton(colorConnectorsBtnData);

            // ----------------------------
            // ----------------------------
            img = Properties.Resources.icons8_doctors_folder_32;
            imageSource = GetImageSource(img);

            var endConditionFillParameterData =
                new PushButtonData("endConditionFillParameter", "Заполнить \"Концевое условие\"", this.path + "\\Gladkoe.dll", "Gladkoe.EndConditionFillParameter")
                {
                    ToolTip = "Заполнение параметров \"Концевое условие\" (сварной шов / фланец) в проекте НБ_Гладкое",
                    LongDescription = "Разработчик: Кожевников Андрей",
                    Image = imageSource,
                    LargeImage = imageSource
                };

            pullDownButtonGladkoe.AddPushButton(endConditionFillParameterData);

            // ----------------------------
            // ----------------------------
            var lineSectionNumberFillParameterData =
                new PushButtonData(
                    "lineSectionNumberFillParameter",
                    "Заполнить \"Номер участка линии\"",
                    this.path + "\\Gladkoe.dll",
                    "Gladkoe.LineSectionNumberFillParameter.LineSectionNumberFillParameter")
                {
                    ToolTip = "Заполнение параметра \"Номер участка линии\" в проекте НБ_Гладкое",
                    LongDescription = "Разработчик: Кожевников Андрей",
                    Image = imageSource,
                    LargeImage = imageSource
                };

            pullDownButtonGladkoe.AddPushButton(lineSectionNumberFillParameterData);

            // ----------------------------
            // ----------------------------
            var parameterManipulationButtonData =
                new PushButtonData(
                    "parameterManipulation",
                    "Сохранение пар-ров в файл + заполнение труб",
                    this.path + "\\Gladkoe.dll",
                    "Gladkoe.ParameterDataManipulations.FillParameters")
                {
                    ToolTip = "Заполнение параметров \"ID\", \"Длина\", \"Наружный диаметр\", \"Условный диаметр\" и сохранение параметров категорий:\n"
                              + $"\"Арматура трубопроводов\",\n "
                              + $"\"Оборудование\", \n"
                              + $"\"Гибкие трубы\", \n"
                              + $"\"Сантехнические приборы\", \n"
                              + $"\"Соединительные детали трубопроводов\" \n \n в файл",

                    LongDescription = "Разработчик: Кожевников Андрей",
                    Image = imageSource,
                    LargeImage = imageSource
                };

            pullDownButtonGladkoe.AddPushButton(parameterManipulationButtonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private static BitmapImage GetImageSource(Image img)
        {
            var bmp = new BitmapImage();

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
    }
}