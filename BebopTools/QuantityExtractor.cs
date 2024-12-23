using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ParameterUtils;
using BebopTools.DownloadAndUploadUtils;
using BebopTools.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using BebopTools.SelectionUtils;
using BebopTools.GeometryUtils;


namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    internal class QuantityExtractor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            //List that will contain the surfaces that will be measured, if the surfaces option is selected
            List<string> surfaces = new List<string>() {};

            //String that will contain the type of elements measured (surfaces, lines or volumes)
            string selectedOption ="";

            //Quantity Calculator instance
            QuantityCalculator quantityCalculator = new QuantityCalculator(doc);


            ElementSelectorForQuantities elementSelectorForQuantities = new ElementSelectorForQuantities();

            if (elementSelectorForQuantities.ShowDialog() == true)
            {
                selectedOption = elementSelectorForQuantities.SelectionOption;

                if (!string.IsNullOrWhiteSpace(selectedOption) && selectedOption == "Áreas")
                {
                    ElementSurfaceSelector elementSurfaceSelector = new ElementSurfaceSelector();   

                    if(elementSurfaceSelector.ShowDialog() == true)
                    {
                        surfaces = elementSurfaceSelector.Surfaces;
                    }

                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(selectedOption))
                {

                    if (selectedOption == "Áreas")
                    {
                        if (surfaces.Count > 0)
                        {
                            var references = uidoc.Selection.PickObjects(ObjectType.Face, new SurfaceSelector(surfaces, doc));
                            if (references == null || references.Count == 0)
                            {
                                TaskDialog.Show("Error", "No elements were selected.");
                                return Result.Failed;
                            }

                            double total = quantityCalculator.AreaCalculator(references);
                            TaskDialog.Show("Success", $"The total area of the selected faces is {Math.Round(total,3)} m²");
                            return Result.Succeeded;
                        }
                        else
                        {
                            TaskDialog.Show("Error", "No surfaces were selected");
                            return Result.Failed;
                        }
                    }
                    else if (selectedOption == "Volúmenes")
                    {
                        var references = uidoc.Selection.PickObjects(ObjectType.Element, new VolumeSelector(doc));
                        if (references == null || references.Count == 0)
                        {
                            TaskDialog.Show("Error", "No elements were selected.");
                            return Result.Failed;
                        }
                        double total = quantityCalculator.VolumeCalculator(references);
                        TaskDialog.Show("Success", $"The total volume of the selected objects is {Math.Round(total, 3)} m³");
                        return Result.Succeeded;
                    }

                    else if (selectedOption == "Líneas")
                    {
                        var references = uidoc.Selection.PickObjects(ObjectType.Element, new LineSelector (doc));
                        if (references == null || references.Count == 0)
                        {
                            TaskDialog.Show("Error", "No elements were selected.");
                            return Result.Failed;
                        }
                        double total = quantityCalculator.LengthCalculator(references);
                        TaskDialog.Show("Success", $"The total length of the selected lines is {Math.Round(total, 3)} m");
                        return Result.Succeeded;

                    }




                }
                else
                {
                    TaskDialog.Show("Error", "No type of quantity was selected");
                    return Result.Failed;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                TaskDialog.Show("Info", "Selection canceled by the user.");
                return Result.Failed;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error of type {ex.GetType()} ocurred {ex.ToString()}");
                return Result.Failed;
            }
 

            return Result.Succeeded;
        }
    }
}
