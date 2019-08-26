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

    /// <summary>
    /// The create shared parameter.
    /// </summary>
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

                List<RevitParameter> dataList = HelperParams.LoadExcel();

                bool showResult = false;

                if (dataList != null)
                {
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start($"Adding Shared Parameters");

                        foreach (var item in dataList)
                        {
                            DefinitionGroup dg = HelperParams.GetOrCreateSharedParamsGroup(sharedParameterFile, item.GroupName);
                            ExternalDefinition externalDefinition = HelperParams.GetOrCreateSharedParamDefinition(dg, item.ParamType, item.ParamName, item.IsVisible);

                            Category category = doc.Settings.Categories.get_Item(item.Category);
                            categorySet.Insert(category);

                            // parameter binding
                            Binding newIb;
                            if (item.IsInstance)
                            {
                                newIb = app.Create.NewInstanceBinding(categorySet);
                            }
                            else
                            {
                                newIb = app.Create.NewTypeBinding(categorySet);
                            }

                            doc.ParameterBindings.Insert(externalDefinition, newIb, item.ParamGroup);

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
                        TaskDialog.Show("Adding Shared Parameters", "Shared parameters have been added successfully");
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Add Shared Parameters", e.Message);
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