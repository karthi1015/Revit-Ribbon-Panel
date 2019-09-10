namespace Gladkoe
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FillingParameters : IExternalCommand
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

                SetParameters(doc, flanges, "Концевое условие", "Фланец", "Концевое условие 2", "Сварной шов");
                SetParameters(doc, welds, "Концевое условие", "Сварной шов", "Концевое условие 2", "Сварной шов");

                tran.Commit();
            }

            TaskDialog.Show("Fill parameters", "Параметры заполнены");
        }

        private static void SetParameters(
            Document doc,
            List<ElementId> elementsId,
            string parameterName,
            string parameterValue,
            string parameterName2 = null,
            string parameterValue2 = null)
        {
            IEnumerable<Element> elements = elementsId.Select(doc.GetElement);

            foreach (Element element in elements)
            {
                Parameter p = element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals(parameterName)) ?? throw new ArgumentNullException(
                                  nameof(p),
                                  $"Проблема в нахождении параметра \"{parameterName}\", проверьте наименования параметров");
                p.Set(parameterValue);
                if (parameterName2 != null)
                {
                    p = element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals(parameterName2)) ?? throw new ArgumentNullException(
                            nameof(p),
                            $"Проблема в нахождении параметра \"{parameterName2}\", проверьте наименования параметров");
                    p.Set(parameterValue2);
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