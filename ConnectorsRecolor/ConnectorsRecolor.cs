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
    public class ConnectorsRecolor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            ChangeColor(doc);

            return Result.Succeeded;
        }

        private static void ChangeColor(Document doc)
        {
            try
            {
                ChangeColorOneQuery(doc);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Recolor", ex.Message);
            }
        }

        private static ElementId GetMaterialId(Document doc, string pipeType)
        {
            switch (pipeType)
            {
                case "Азот_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("0_153_255"))?.Id;
                case "Вода_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("0_96_0"))?.Id;
                case "Газ_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("255_220_112"))?.Id;
                case "Дренаж_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("192_192_192"))?.Id;
                case "Канализация_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("192_192_192"))?.Id;
                case "Нефтепродукты_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("160_80_0"))?.Id;
                case "Пенообразователь_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("224_0_0"))?.Id;
                case "ХимическиеРеагенты_":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("128_96_0"))?.Id;
            }

            return null;
        }

        private static void SetColor(IEnumerable<Element> elements, ElementId materialId)
        {
            foreach (Element element in elements)
            {
                Parameter p = element.GetOrderedParameters().FirstOrDefault(e => e.Definition.Name.Equals("МатериалФитинга"));
                p?.Set(materialId);
            }
        }

        private static Dictionary<string, List<Element>> GetElements(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(i => i.Name.Equals("ГОСТ 10704-91 Трубы стальные электросварные прямошовные") && i.Symbol.FamilyName.Equals("801_СварнойШов_ОБЩИЙ"))
                .SelectMany(e => e.MEPModel.ConnectorManager.Connectors.Cast<Connector>(), (e, connector) => (familyInstance: e, connector))
                .SelectMany(t => t.connector.AllRefs.Cast<Connector>(), (familyInstance, reference) => (familyInstance, reference))
                .Where(
                    t => t.reference.Owner.Name.StartsWith("Азот_")
                         || t.reference.Owner.Name.StartsWith("Вода_")
                         || t.reference.Owner.Name.StartsWith("Газ_")
                         || t.reference.Owner.Name.StartsWith("Дренаж_")
                         || t.reference.Owner.Name.StartsWith("Канализация_")
                         || t.reference.Owner.Name.StartsWith("Нефтепродукты_")
                         || t.reference.Owner.Name.StartsWith("Пенообразователь_")
                         || t.reference.Owner.Name.StartsWith("ХимическиеРеагенты_"))
                .Select(t => (t.familyInstance.familyInstance, t.reference.Owner))
                .SelectMany(
                    e => e.familyInstance.MEPModel.ConnectorManager.Connectors.Cast<Connector>(),
                    (e, connector) => (familyInstance: e.familyInstance, connector, Owner: e.Owner))
                .SelectMany(t => t.connector.AllRefs.Cast<Connector>(), (t, reference) => (InstanceConnectorOwnerTuple: t, reference))
                .Where(
                    t => (t.reference.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                         && !t.reference.Owner.Name.Equals("ГОСТ 10704-91 Трубы стальные электросварные прямошовные"))
                .GroupBy(e => e.InstanceConnectorOwnerTuple.Owner.Name, e => e.reference.Owner)
                .ToDictionary(e => e.Key, e => e.ToList());
        }

        private static int ChangeColor(Document doc, Dictionary<string, List<Element>> pipes, string pipeType)
        {
            int count = 0;

            foreach (KeyValuePair<string, List<Element>> valuePair in pipes)
            {
                if (valuePair.Key.StartsWith(pipeType))
                {
                    ElementId material = GetMaterialId(doc, pipeType) ?? throw new ArgumentNullException(
                                             nameof(material),
                                             "Проблема в нахождении материалов, проверьте наименования материалов");
                    SetColor(valuePair.Value, material);
                    count += valuePair.Value.Count;
                }
            }

            return count;
        }

        private static void ChangeColorOneQuery(Document doc)
        {
            var sw = Stopwatch.StartNew();

            Dictionary<string, List<Element>> pipes = GetElements(doc) ?? throw new ArgumentNullException(
                                                          nameof(pipes),
                                                          "Проблема в нахождении коннекторов, проверьте наименования семейств");
            int count;
            using (Transaction tran = new Transaction(doc))
            {
                tran.Start("Change color");

                count = ChangeColor(doc, pipes, "Азот_") +
                        ChangeColor(doc, pipes, "Вода_") +
                        ChangeColor(doc, pipes, "Газ_") +
                        ChangeColor(doc, pipes, "Дренаж_") +
                        ChangeColor(doc, pipes, "Канализация_") +
                        ChangeColor(doc, pipes, "Нефтепродукты_") +
                        ChangeColor(doc, pipes, "Пенообразователь_") +
                        ChangeColor(doc, pipes, "ХимическиеРеагенты_");

                tran.Commit();
            }

            sw.Stop();

            TaskDialog.Show("Recolor", $"{count} elements proceed " + $"in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }
    }
}