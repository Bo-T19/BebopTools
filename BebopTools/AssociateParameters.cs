#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ParameterUtils;
using BebopTools.SelectionUtils;
using BebopTools.UploadUtils;
using BebopTools.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace BebopTools
{
    //COMPLETAR
    [Transaction(TransactionMode.Manual)]
    public class AssociateParameters : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Path of the excel file, name of the parameter to be edited
            string selectedPath = "";
            string selectedTargetParameter = "";
            string selectedSourceParameter = "";

            //Create the parametersManager, which contains methods used later
            ParametersManager parametersManager = new ParametersManager(doc);

            //Open the fileSelector and get its values
            FileSelectorForAssociateParams fileSelector = new FileSelectorForAssociateParams(parametersManager);
            if (fileSelector.ShowDialog() == true)
            {
                selectedPath = fileSelector.Path;
                selectedTargetParameter = fileSelector.SelectedTargetParameter;
                selectedSourceParameter = fileSelector.SelectedSourceParameter;

            }

            //Get all the model elements
            ElementsSelector elementSelector = new ElementsSelector(doc);
            IEnumerable<ElementId> modelElements = elementSelector.GetModelElementsInstances();

            //Try to handle the scenario where there is no path and no parameters
            try
            {
                if (!string.IsNullOrWhiteSpace(selectedPath) 
                    && !string.IsNullOrWhiteSpace(selectedTargetParameter)
                    && !string.IsNullOrWhiteSpace(selectedSourceParameter))
                {
                    //Dictionary loaded
                    FileUploader fileUploader = new FileUploader(selectedPath);
                    Dictionary<string, string> parameterDictionary = fileUploader.GetInfo();
                    foreach (ElementId elementId in modelElements)
                    {
                        parametersManager.AssociateParameters(elementId, parameterDictionary, selectedSourceParameter, selectedTargetParameter);
                    }

                    return Result.Succeeded;
                }
                else
                {
                    TaskDialog.Show("Error", "No file was selected and/or parameter were selected");
                    return Result.Failed;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "An error ocurred");
                return Result.Failed;
            }

            //Iterate over the elements and fill the parameters


        }
    }
}
