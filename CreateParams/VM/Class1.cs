// ReSharper disable All
namespace CreateParams.VM
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    class CmdProjectParameterGuids : IExternalCommand
    {
        class ProjectParameterData
        {
            public Definition Definition = null;

            public ElementBinding Binding = null;

            public string Name = null; 

            public string GUID = null;
        }
        
        private static List<ProjectParameterData> GetProjectParameterData(Document doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            if (doc.IsFamilyDocument)
            {
                throw new Exception("doc can not be a family document.");
            }

            List<ProjectParameterData> result = new List<ProjectParameterData>();

            BindingMap map = doc.ParameterBindings;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                ProjectParameterData newProjectParameterData = new ProjectParameterData
                {
                    Definition = it.Key,
                    Name = it.Key.Name,
                    Binding = it.Current as ElementBinding
                };

                result.Add(newProjectParameterData);
            }

            return result;
        }

        /// <summary>
        /// This method takes a category and information 
        /// about a project parameter and adds a binding 
        /// to the category for the parameter.  It will 
        /// throw an exception if the parameter is already 
        /// bound to the desired category.  It returns
        /// whether or not the API reports that it 
        /// successfully bound the parameter to the 
        /// desired category.
        /// </summary>
        /// <param name="doc">The project document in which the project parameter has been defined</param>
        /// <param name="projectParameterData">Information about the project parameter</param>
        /// <param name="category">The additional category to which to bind the project parameter</param>
        /// <returns></returns>
        static bool AddProjectParameterBinding(Document doc, ProjectParameterData projectParameterData, Category category)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            if (doc.IsFamilyDocument)
            {
                throw new Exception("doc can not be a family document.");
            }

            if (projectParameterData == null)
            {
                throw new ArgumentNullException("projectParameterData");
            }

            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            bool result = false;

            CategorySet cats = projectParameterData.Binding.Categories;

            if (cats.Contains(category))
            {
                // It's already bound to the desired category. Nothing to do.
                string errorMessage = string.Format("The project parameter '{0}' is already bound to the '{1}' category.", projectParameterData.Definition.Name, category.Name);

                throw new Exception(errorMessage);
            }

            cats.Insert(category);

            // See if the parameter is an instance or type parameter.
            InstanceBinding instanceBinding = projectParameterData.Binding as InstanceBinding;

            if (instanceBinding != null)
            {
                // Is an Instance parameter
                InstanceBinding newInstanceBinding = doc.Application.Create.NewInstanceBinding(cats);

                if (doc.ParameterBindings.ReInsert(projectParameterData.Definition, newInstanceBinding))
                {
                    result = true;
                }
            }
            else
            {
                // Is a type parameter
                TypeBinding typeBinding = doc.Application.Create.NewTypeBinding(cats);

                if (doc.ParameterBindings.ReInsert(projectParameterData.Definition, typeBinding))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// This method populates the appropriate values 
        /// on a ProjectParameterData object with 
        /// information from the given Parameter object.
        /// </summary>
        /// <param name="parameter">The Parameter object with source information</param>
        /// <param name="projectParameterDataToFill">The ProjectParameterData object to fill</param>
        static void PopulateProjectParameterData(Parameter parameter, ProjectParameterData projectParameterDataToFill)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (projectParameterDataToFill == null)
            {
                throw new ArgumentNullException("projectParameterDataToFill");
            }

            if (parameter.IsShared)
            {
                if (parameter.GUID != null)
                {
                    projectParameterDataToFill.GUID = parameter.GUID.ToString();
                }
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                message = "The document must be a project document.";
                return Result.Failed;
            }

            Element firstWallTypeElement = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().FirstElement();

            CategorySet categories = null;
            Parameter foundParameter = null;

            List<ProjectParameterData> projectParametersData = GetProjectParameterData(doc);

            foreach (ProjectParameterData projectParameterData in projectParametersData)
            {
                categories = projectParameterData.Binding.Categories;

                using (Transaction tempTransaction = new Transaction(doc))
                {
                    tempTransaction.Start("Temporary");

                    if (!categories.Contains(firstWallTypeElement.Category))
                    {
                        if (AddProjectParameterBinding(doc, projectParameterData, firstWallTypeElement.Category))
                        {
                            foundParameter = firstWallTypeElement.get_Parameter(projectParameterData.Definition);
                        }
                    }
                    else
                    {
                        foundParameter = firstWallTypeElement.get_Parameter(projectParameterData.Definition);
                    }

                    if (foundParameter != null)
                    {
                        PopulateProjectParameterData(foundParameter, projectParameterData);
                    }

                    tempTransaction.RollBack();
                }
            }

            return Result.Succeeded;
        }
    }
}