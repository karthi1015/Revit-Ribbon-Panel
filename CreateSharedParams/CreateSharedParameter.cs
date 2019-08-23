// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSharedParameter.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Defines the CreateSharedParameter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable StyleCop.SA1600
// ReSharper disable StyleCop.SA1402

namespace CreateSharedParams
{
    using System;
    using System.Collections.Generic;

    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using CreateSharedParams.HelperClass;
    using CreateSharedParams.Models;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateSharedParameter : IExternalCommand
    {
        /// <summary>
        /// The create project shared parameters.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="app">
        /// The app.
        /// </param>
        public static void CreateProjectSharedParameter(Document doc, Application app)
        {
            CategorySet categorySet = app.Create.NewCategorySet();
            try
            {
                DefinitionFile sharedParameterFile = HelperParams.GetOrCreateSharedParamsFile(doc, app);

                List<SharedParameter> dataList = HelperParams.LoadExcel();

                bool showResult = false;

                using (Transaction t = new Transaction(doc))
                {
                    t.Start($"Add Shared Parameters");

                    foreach (var item in dataList)
                    {
                        DefinitionGroup dg = HelperParams.GetOrCreateSharedParamsGroup(
                            sharedParameterFile,
                            item.GroupName);
                        var parameterTypeParse = (ParameterType)Enum.Parse(typeof(ParameterType), item.ParamType);
                        var visibleParse = bool.Parse(item.Visible);
                        ExternalDefinition externalDefinition = HelperParams.GetOrCreateSharedParamDefinition(
                            dg,
                            parameterTypeParse,
                            item.ParamName,
                            visibleParse);

                        var categoryParse = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), item.Category);

                        Category category = doc.Settings.Categories.get_Item(categoryParse);
                        categorySet.Insert(category);

                        // parameter group to text
                        var parameterGroupParse = (BuiltInParameterGroup)Enum.Parse(
                            typeof(BuiltInParameterGroup),
                            item.ParamGroup);

                        // parameter binding
                        Binding newIb;
                        if (bool.Parse(item.Instance))
                        {
                            newIb = app.Create.NewInstanceBinding(categorySet);
                        }
                        else
                        {
                            newIb = app.Create.NewTypeBinding(categorySet);
                        }

                        doc.ParameterBindings.Insert(externalDefinition, newIb, parameterGroupParse);

                        if (doc.ParameterBindings.Contains(externalDefinition))
                        {
                            if (doc.ParameterBindings.ReInsert(externalDefinition, newIb))
                            {
                                showResult = true;
                            }
                        }
                    }

                    t.Commit();
                }

                if (showResult)
                {
                    TaskDialog.Show("Revit", "Successfully bound");
                }
            }
            catch
            {
                // ignored
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            CreateProjectSharedParameter(doc, app);

            return Result.Succeeded;
        }
    }
}
