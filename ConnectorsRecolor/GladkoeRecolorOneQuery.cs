using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ConnectorsRecolor
{
    public class GladkoeRecolorOneQuery
    {
        public static void ChangeColor(Document doc)
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
                case "Азот":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("0_153_255"))?.Id;
                case "Вода":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("0_96_0"))?.Id;
                case "Газ":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("255_220_112"))?.Id;
                case "Дренаж":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("192_192_192"))?.Id;
                case "Канализация":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("192_192_192"))?.Id;
                case "Нефтепродукты":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("160_80_0"))?.Id;
                case "Пенообразователь":
                    return new FilteredElementCollector(doc).OfClass(typeof(Material)).FirstOrDefault(m => m.Name.Equals("224_0_0"))?.Id;
                case "ХимическиеРеагенты":
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
                    t => t.reference.Owner.Name.Contains("Азот") || t.reference.Owner.Name.Contains("Вода") || t.reference.Owner.Name.Contains("Газ")
                         || t.reference.Owner.Name.Contains("Дренаж") || t.reference.Owner.Name.Contains("Канализация")
                         || t.reference.Owner.Name.Contains("Нефтепродукты") || t.reference.Owner.Name.Contains("Пенообразователь")
                         || t.reference.Owner.Name.Contains("ХимическиеРеагенты"))
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
                if (valuePair.Key.Contains(pipeType))
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

                count = ChangeColor(doc, pipes, "Азот") + ChangeColor(doc, pipes, "Вода") + ChangeColor(doc, pipes, "Газ") + ChangeColor(doc, pipes, "Дренаж")
                        + ChangeColor(doc, pipes, "Канализация") + ChangeColor(doc, pipes, "Нефтепродукты") + ChangeColor(doc, pipes, "Пенообразователь")
                        + ChangeColor(doc, pipes, "ХимическиеРеагенты");

                tran.Commit();
            }

            sw.Stop();

            TaskDialog.Show("Recolor", $"{count} elements proceed " + $"in {sw.Elapsed.TotalSeconds:F2} seconds.");
        }
    }
}