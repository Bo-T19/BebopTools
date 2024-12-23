#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ParameterUtils;
using BebopTools.UploadUtils;
using BebopTools.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

#endregion;

namespace BebopTools
{

    //PRoblemas: no está leyendo bien la hoja de excel, no está trayendo los element ids como element ids
    [Transaction(TransactionMode.Manual)]
    public class FillParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Path of the excel file, name of the parameter to be edited
            string selectedPath ="";
            string selectedParameter ="";

            //Create the parametersManager, which contains methods used later
            ParametersManager parametersManager = new ParametersManager(doc);

            //Open the fileSelector and get its values
            FileSelectorForFillParams fileSelector = new FileSelectorForFillParams(parametersManager);
            if (fileSelector.ShowDialog() == true)
            {
                selectedPath = fileSelector.Path;
                selectedParameter = fileSelector.SelectedParameter;
            }

            //Try-catch to handle the scenario where there is no path
            try
            {
                if (!string.IsNullOrWhiteSpace(selectedPath) && !string.IsNullOrWhiteSpace(selectedParameter))
                {
                    //Dictionary loaded
                    FileUploader fileUploader = new FileUploader(selectedPath);
                    Dictionary<string, string> parameterDictionary = fileUploader.GetInfo();
                    parametersManager.FillParameters(parameterDictionary, selectedParameter);

                    return Result.Succeeded;
                }
                else
                {
                    TaskDialog.Show("Error","No file was selected and/or parameter were selected");
                    return Result.Failed;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error of type {ex.GetType()} ocurred {ex.ToString()}");
                return Result.Failed;
            }
        }
    }
}
