namespace Gladkoe.ParameterDataManipulations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Plumbing;
    using Autodesk.Revit.UI;

    public static class CopyParameters
    {
        public static Document RevitDocument { get; private set; }

        public static UIDocument UiRevitDocument { get; private set; }

        public static void FillParams(Document doc, UIDocument uidoc)
        {
            RevitDocument = doc;
            UiRevitDocument = uidoc;

            try
            {
                FillParametersAction();
            }
            catch (Exception e)
            {
                TaskDialog.Show("Fill parameters", e.Message);
            }
        }

        private static void FillParametersAction()
        {
            List<Element> elements = GetElements();

            using (Transaction tran = new Transaction(RevitDocument))
            {
                tran.Start("Заполнить ID, Длину, Наружный диаметр, Условный диаметр");

                SetParameters(elements);

                tran.Commit();
            }
        }

        private static Parameter GetParameter(Element element, string parameterName)
        {
            return element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals(parameterName))
                   ?? throw new ArgumentException($"Проблема в нахождении параметра \"{parameterName}\", проверьте верность наименования и наличие параметров");
        }

        private static void SetParameters(List<Element> elements)
        {
            int i = 0;

            foreach (Element element in elements)
            {
                Parameter resultParameter = GetParameter(element, "UID");
                resultParameter.Set(element.Id.IntegerValue.ToString());

                if (element is Pipe pipe)
                {
                    Parameter p1 = pipe.GetOrderedParameters()
                        .Where(p => !p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Длина"));
                    resultParameter = pipe.GetOrderedParameters()
                        .Where(p => p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Длина"));
                    if (p1 != null)
                    {
                        resultParameter?.Set(p1.AsDouble());
                    }

                    p1 = pipe.GetOrderedParameters()
                        .Where(p => !p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Внешний диаметр"));
                    resultParameter = pipe.GetOrderedParameters()
                        .Where(p => p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Наружный диаметр"));
                    if (p1 != null)
                    {
                        resultParameter?.Set(p1.AsDouble());
                    }

                    p1 = pipe.GetOrderedParameters()
                        .Where(p => !p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_MECHANICAL))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Диаметр"));
                    resultParameter = pipe.GetOrderedParameters()
                        .Where(p => p.IsShared && (p.Definition.ParameterGroup == BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES))
                        .FirstOrDefault(p => p.Definition.Name.Equals("Условный диаметр"));
                    if (p1 != null)
                    {
                        resultParameter?.Set(p1.AsDouble());
                    }
                }

                i++;
            }

            TaskDialog.Show("Info", $"Элементов обработано {i}");
        }

        private static List<Element> GetElements()
        {
            return new FilteredElementCollector(RevitDocument).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(
                    new ElementMulticategoryFilter(
                        new List<BuiltInCategory>
                        {
                            BuiltInCategory.OST_PipeAccessory,
                            BuiltInCategory.OST_PipeCurves,
                            BuiltInCategory.OST_MechanicalEquipment,
                            BuiltInCategory.OST_PipeFitting,
                            BuiltInCategory.OST_FlexPipeCurves,
                            BuiltInCategory.OST_PlumbingFixtures
                        }))
                .ToElements()
                .Where(
                    delegate(Element e)
                    {
                        Parameter volume = e.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);

                        if ((e is FamilyInstance fs && (fs.SuperComponent != null)) || ((volume != null) && !volume.HasValue))
                        {
                            return false;
                        }

                        return true;
                    })
                .ToList();
        }
    }
}