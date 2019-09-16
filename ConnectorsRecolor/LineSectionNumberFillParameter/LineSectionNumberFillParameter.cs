using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Gladkoe.Utilities;

namespace Gladkoe.LineSectionNumberFillParameter
{
    using Autodesk.Revit.ApplicationServices;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LineSectionNumberFillParameter : IExternalCommand
    {
        public static Document RevitDocument { get; private set; }

        public static void FillParams(Document doc)
        {
            RevitDocument = doc;

            try
            {
                FillParametersAction();
            }
            catch (Exception e)
            {
                TaskDialog.Show("Fill parameters", e.Message);
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            FillParams(doc);

            return Result.Succeeded;
        }

        private static void FillParametersAction()
        {
            List<Element> elements = GetElements();

            using (Transaction tran = new Transaction(RevitDocument))
            {
                tran.Start("Заполнить номер участка линии");

                SetParameters(elements);

                tran.Commit();
            }
        }

        private static Parameter GetParameter(Element element, string parameterName)
        {
            return element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals(parameterName))
                   ?? throw new ArgumentException(
                       $"Проблема в нахождении параметра \"{parameterName}\", проверьте верность наименования и наличие параметров.\n"
                       + $"Необходимо, чтобы категории:\n"
                       + $"\"Трубы\",\n "
                       + $"\"Арматура трубопроводов\", \n"
                       + $"\"Соединительные детали трубопроводов\" \n \n"
                       + $"содержали параметры:\n \n"
                       + $"\"Номер участка линии\", \n"
                       + $"\"№ поз. по ГП\", \n"
                       + $"\"Шифр продукта\", \n"
                       + $"\"Номер по технологической схеме\", \n"
                       + $"\"Условный диаметр\"(тип - длина), \n"
                       + $"\"Условное давление\" (тип - давление), \n"
                       + $"\"Конструкция трубопровода\"");
        }

        private static void SetParameters(List<Element> elements)
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (Element element in elements)
            {
                Parameter resultParameter = GetParameter(element, "Номер участка линии");

                string s1 = GetParameter(element, "№ поз. по ГП").GetParameterValue();
                string s2 = GetParameter(element, "Шифр продукта").GetParameterValue();
                string s3 = GetParameter(element, "Номер по технологической схеме").GetParameterValue();
                string s4 = GetParameter(element, "Условный диаметр").GetParameterValue().Split(' ').FirstOrDefault();
                string s5 = GetParameter(element, "Условное давление").GetParameterValue().Split(' ').FirstOrDefault();
                string s6 = GetParameter(element, "Конструкция трубопровода").GetParameterValue();

                if ((s1 != string.Empty) && (s2 != string.Empty) && (s3 != string.Empty) && (s4 != string.Empty) && (s5 != string.Empty) && (s6 != string.Empty))
                {
                    resultParameter.Set($"{s1}-{s2}-{s3}-{s4}-{s5}-{s6}");
                    i++;
                    continue;
                }

                if ((s1 != string.Empty) && (s2 != string.Empty) && (s3 == string.Empty) && (s4 != string.Empty) && (s5 != string.Empty) && (s6 != string.Empty))
                {
                    resultParameter.Set($"{s1}-{s2}-{s4}-{s5}-{s6}");
                    i++;
                }
                else if ((s1 == string.Empty) || (s2 == string.Empty) || (s4 == string.Empty) || (s5 == string.Empty) || (s6 == string.Empty))
                {
                    sb.Append(
                        $"element ID: {element.Id.IntegerValue.ToString()}, не заполнены: "
                        + $"{(s1 != string.Empty ? string.Empty : GetParameter(element, "№ поз. по ГП").Definition.Name + ",")}"
                        + $"{(s2 != string.Empty ? string.Empty : GetParameter(element, "Шифр продукта").Definition.Name + ",")}"
                        + $"{(s4 != string.Empty ? string.Empty : GetParameter(element, "Условный диаметр").Definition.Name + ",")}"
                        + $"{(s5 != string.Empty ? string.Empty : GetParameter(element, "Условное давление").Definition.Name + ",")}"
                        + $"{(s6 != string.Empty ? string.Empty : GetParameter(element, "Конструкция трубопровода").Definition.Name)}");
                    sb.AppendLine();
                }
            }

            var window = new ResultWindow(sb, i);
            window.ShowDialog();
        }

        private static List<Element> GetElements()
        {
            return new FilteredElementCollector(RevitDocument).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(
                    new ElementMulticategoryFilter(
                        new List<BuiltInCategory> { BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeCurves, BuiltInCategory.OST_PipeAccessory }))
                .ToElements()
                .ToList();
        }
    }
}