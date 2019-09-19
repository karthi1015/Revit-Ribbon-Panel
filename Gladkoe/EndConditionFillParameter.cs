namespace Gladkoe
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class EndConditionFillParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            FillParams(doc, uidoc);

            return Result.Succeeded;
        }

        private static void FillParams(Document doc, UIDocument uidoc)
        {
            try
            {
                FillParametersAction(doc);
            }
            catch (Exception e)
            {
                TaskDialog.Show("Fill parameters", e.Message);
            }
        }

        private static void FillParametersAction(Document doc)
        {
            // концевое условие - фланец
            List<ElementId> flanges = GetFlange(doc);

            // концевое условие - сварка
            List<ElementId> welds = GetWelds(doc, flanges);
            using (Transaction tran = new Transaction(doc))
            {
                tran.Start("Fill parameters");

                SetFlangesParameters(doc, flanges, "Фланец", "Сварной шов", "Сварной шов", "Сварной шов");

                SetWeldParameters(doc, welds);

                tran.Commit();
            }

            TaskDialog.Show("Fill parameters", "Параметры заполнены");
        }

        private static Parameter GetParameter(Element element, string parameterName)
        {
            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine($"Проблема в нахождении параметра \"{parameterName}\", проверьте верность наименования и наличие параметров.");
            sb2.AppendLine();
            sb2.AppendLine("Необходимо, чтобы категории:");
            sb2.AppendLine("\"Оборудование\",");
            sb2.AppendLine("\"Арматура трубопроводов\",");
            sb2.AppendLine();
            sb2.AppendLine("содержали параметры:");
            sb2.AppendLine("\"Концевое условие\",");
            sb2.AppendLine("\"Концевое условие 2\",");
            sb2.AppendLine("\"Концевое условие 3\",");
            sb2.AppendLine("\"Концевое условие 4\",");

            return element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals(parameterName)) ?? throw new ArgumentNullException(parameterName,sb2.ToString());
        }

        private static void SetWeldParameters(Document doc, List<ElementId> elementIds)
        {
            IEnumerable<Element> elements = elementIds.Select(doc.GetElement);

            foreach (Element element in elements)
            {
                Parameter p = GetParameter(element, "Концевое условие");
                p.Set("Сварной шов");

                p = GetParameter(element, "Концевое условие 2");
                p.Set("Сварной шов");

                if (element is FamilyInstance fs && (fs.Symbol.FamilyName.Contains("Тройник") || fs.Symbol.FamilyName.Contains("Фильтр")))
                {
                    p = GetParameter(element, "Концевое условие 3");
                    p.Set("Сварной шов");

                    if (fs.Symbol.FamilyName.Contains("Крестовина"))
                    {
                        p = GetParameter(element, "Концевое условие 3");
                        p.Set("Сварной шов");
                        p = GetParameter(element, "Концевое условие 4");
                        p.Set("Сварной шов");
                    }
                }
            }
        }

        private static void SetFlangesParameters(Document doc, List<ElementId> elementsId, params string[] values)
        {
            IEnumerable<Element> elements = elementsId.Select(doc.GetElement);

            foreach (Element element in elements)
            {
                Parameter p = GetParameter(element, "Концевое условие");
                p.Set(values[0]);

                p = GetParameter(element, "Концевое условие 2");
                p.Set(values[1]);

                if (element is FamilyInstance fs && (fs.Symbol.FamilyName.Contains("Тройник") || fs.Symbol.FamilyName.Contains("Фильтр")))
                {
                    p = GetParameter(element, "Концевое условие 3");
                    p.Set(values[2]);

                    if (fs.Symbol.FamilyName.Contains("Крестовина"))
                    {
                        p = GetParameter(element, "Концевое условие 3");
                        p.Set(values[2]);
                        p = GetParameter(element, "Концевое условие 4");
                        p.Set(values[3]);
                    }
                }
            }
        }

        private static List<ElementId> GetWelds(Document doc, List<ElementId> exceptFlange)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(new ElementMulticategoryFilter(new List<BuiltInCategory> { BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeCurves }))
                .ToElements()
                .Where(
                    e =>
                    {
                        if (doc.GetElement(e.Id) is FamilyInstance s)
                        {
                            return !s.Symbol.FamilyName.Equals("801_СварнойШов_ОБЩИЙ") && (s.SuperComponent == null) && (s.MEPModel.ConnectorManager != null)
                                   && !s.Symbol.FamilyName.Equals("802_ОпорыКорпусныеПриварные_КП_ОСТ36-146-88(ОбМод)");
                        }

                        return true;
                    })
                .Select(e => e.Id)
                .Except(exceptFlange)
                .ToList();
        }

        private static IEnumerable<ElementId> GetFlangeWithoutWelding(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(new ElementMulticategoryFilter(new List<BuiltInCategory> { BuiltInCategory.OST_PipeAccessory, BuiltInCategory.OST_MechanicalEquipment }))
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(
                    e => (e.SuperComponent == null) && (e.MEPModel.ConnectorManager != null) && !e.Symbol.FamilyName.Equals("802_ОпорыКорпусныеПриварные_КП_ОСТ36-146-88(ОбМод)"))
                .SelectMany(e => e.GetSubComponentIds(), (instance, id) => (instance, subComponent: doc.GetElement(id) as FamilyInstance))
                .Where(f => f.subComponent.Symbol.FamilyName.Contains("_Фланец_"))
                .SelectMany(e => e.instance.MEPModel.ConnectorManager.Connectors.Cast<Connector>(), (instance, connector) => (instance, connector))
                .SelectMany(e => e.connector.AllRefs.Cast<Connector>(), (tupleInstanceConnector, connector) => (tupleInstanceConnector, connector))
                .Where(
                    e =>
                    {
                        if (doc.GetElement(e.connector.Owner.Id) is FamilyInstance s)
                        {
                            return !s.Symbol.FamilyName.Equals("801_СварнойШов_ОБЩИЙ");
                        }

                        return false;
                    })
                .Where(
                    e => (e.connector.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                         || (e.connector.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting))
                .Select(e => e.connector.Owner.Id);
        }

        private static List<ElementId> GetFlange(Document doc)
        {
            IEnumerable<ElementId> flangeWithoutWelding = GetFlangeWithoutWelding(doc);

            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(new ElementMulticategoryFilter(new List<BuiltInCategory> { BuiltInCategory.OST_PipeAccessory, BuiltInCategory.OST_MechanicalEquipment }))
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(
                    e => (e.SuperComponent == null) && (e.MEPModel.ConnectorManager != null) && !e.Symbol.FamilyName.Equals("802_ОпорыКорпусныеПриварные_КП_ОСТ36-146-88(ОбМод)"))
                .SelectMany(e => e.GetSubComponentIds(), (instance, id) => (instance, subComponent: doc.GetElement(id) as FamilyInstance))
                .SelectMany(e => e.instance.MEPModel.ConnectorManager.Connectors.Cast<Connector>(), (instance, connector) => (instance, connector))
                .SelectMany(e => e.connector.AllRefs.Cast<Connector>(), (tupleInstanceConnector, connector) => (tupleInstanceConnector, connector))
                .Where(
                    e =>
                    {
                        if (doc.GetElement(e.connector.Owner.Id) is FamilyInstance s)
                        {
                            return s.Symbol.FamilyName.Equals("801_СварнойШов_ОБЩИЙ");
                        }

                        return false;
                    })
                .SelectMany(e => e.connector.ConnectorManager.Connectors.Cast<Connector>(), (tupleInstanceConnector, connector) => (tupleInstanceConnector, connector))
                .SelectMany(e => e.connector.AllRefs.Cast<Connector>(), (tupleInstanceConnector, connector) => (tupleInstanceConnector, connector))
                .Where(
                    e => (e.connector.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                         || (e.connector.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting))
                .Select(e => e.connector.Owner.Id)
                .Union(flangeWithoutWelding)
                .ToList();
        }
    }
}