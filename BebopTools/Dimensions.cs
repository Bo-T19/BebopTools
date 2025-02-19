using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ModelUtils;
using BebopTools.SelectionUtils;
using BebopTools.WPF;
using BebopTools.WPF.DimensionerWPFs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    public class Dimensions : IExternalCommand
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

            // Get the active view
            View activeView = doc.ActiveView;

            //Ask for the options

            bool dimensionAllWalls = false;
            bool dimensionWidths = false;

            List<ElementId> wallElements;

            WallDimensionerOptions wallDimensionerOptions = new WallDimensionerOptions();
            if (wallDimensionerOptions.ShowDialog() == true)
            {
                dimensionAllWalls = wallDimensionerOptions.DimensionAllWalls;
                dimensionWidths = wallDimensionerOptions.DimensionWidths;
            }

            if(dimensionAllWalls)
            {
                wallElements = new FilteredElementCollector(doc, activeView.Id)
                                .OfCategory(BuiltInCategory.OST_Walls)
                                .WhereElementIsNotElementType()
                                .ToElementIds()
                                .ToList();
            }
            else
            {

                IList<Reference> wallReferences = uidoc.Selection.PickObjects(ObjectType.Element, new SelectorFromCategory(doc, BuiltInCategory.OST_Walls));
                if (wallReferences == null || wallReferences.Count == 0)
                {
                    TaskDialog.Show("Error", "No elements were selected.");
                    return Result.Failed;
                }
                try
                {
                    wallElements = wallReferences.Select(r => r.ElementId).ToList();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    TaskDialog.Show("Info", "Selection canceled by the user.");
                    return Result.Failed;
                }

            }


            Dimensioner dimensioner = new Dimensioner(doc, activeView);

            dimensioner.DimensionFloorPlanWalls(dimensionWidths, wallElements);


            return Result.Succeeded;
        }
    }
}