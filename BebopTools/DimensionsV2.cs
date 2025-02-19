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
    public class DimensionsV2 : IExternalCommand
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
            bool dimensionWidths = false;



            Dimensioner dimensioner = new Dimensioner(doc, activeView);

            dimensioner.DimensionElements(dimensionWidths);


            return Result.Succeeded;
        }
    }
}