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


namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    internal class LevelsManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Variables for the parameter name and the level Name
            List<string>selectedLevels = new List<string>();
            string selectedParameter = "";
            string levelAssociationOption = "";

            //Create the ElementsSelector to get the levels and the ParametersManager to get the parameters
            ElementsSelector elementsSelector = new ElementsSelector(doc);
            ParametersManager parametersManager = new ParametersManager(doc);


            //Create the dialog for selecting the levels and the parameters
            LevelSelector levelSelector = new LevelSelector(elementsSelector, parametersManager);
            if (levelSelector.ShowDialog() == true)
            {
                selectedLevels = levelSelector.SelectedLevels;
                selectedParameter = levelSelector.SelectedParameter;
                levelAssociationOption = levelSelector.SelectionOption;

            }

            //Try-catch to handle the scenario where there are no 
            try
            {
                if (!(selectedLevels.Count==0) && !string.IsNullOrWhiteSpace(selectedParameter))
                {
                    IEnumerable<ElementId> elementIds = elementsSelector.AllElementsInActiveView();
                    List<KeyValuePair<ElementId, (double Elevation, string Name)>> levelsDictionary = elementsSelector.GetLevelsAndElevationsOrdered();
                    parametersManager.AssociateLevels(elementIds, levelsDictionary, selectedParameter, selectedLevels, levelAssociationOption);
                    return Result.Succeeded;
                }
                else
                {
                    TaskDialog.Show("Error", "No levels and/or parameter where selected");
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
