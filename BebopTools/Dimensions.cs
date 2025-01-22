using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.SelectionUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    internal class Dimensions : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            //Active view of the document and instance of the surface selector, which will be used later
            View activeView = doc.ActiveView;

            List<string> selectedSurfaces = new List<string> { "Laterales" };
            SurfaceSelector surfaceSelector = new SurfaceSelector(selectedSurfaces, doc);

            //Getting the walls, columns and stairs
            List<ElementId> elementsToDimension = new List<ElementId>();

            List<ElementId> wallElements = new FilteredElementCollector(doc, activeView.Id)
                                            .OfCategory(BuiltInCategory.OST_Walls) 
                                            .WhereElementIsNotElementType() 
                                            .ToElementIds()
                                            .ToList();

            elementsToDimension.AddRange(wallElements);

            List<ElementId> stairElements = new FilteredElementCollector(doc, activeView.Id)
                                            .OfCategory(BuiltInCategory.OST_Stairs) 
                                            .WhereElementIsNotElementType() 
                                            .ToElementIds()
                                            .ToList();

            elementsToDimension.AddRange (stairElements);

            List<ElementId> columnElements = new FilteredElementCollector(doc, activeView.Id)
                                            .OfCategory(BuiltInCategory.OST_Columns) 
                                            .WhereElementIsNotElementType() 
                                            .ToElementIds()
                                            .ToList();

            elementsToDimension.AddRange (columnElements);

            List<ElementId> structuralColumnElements = new FilteredElementCollector(doc, activeView.Id)
                                            .OfCategory(BuiltInCategory.OST_StructuralColumns)
                                            .WhereElementIsNotElementType()
                                            .ToElementIds()
                                            .ToList();

            elementsToDimension.AddRange (structuralColumnElements);


            //Create the lateral faces list for each element
            List<PlanarFace> lateralFaces = new List<PlanarFace>();

            //Iterate each element in the elements list an get its lateral faces
            foreach (ElementId elementId in elementsToDimension)
            {
                Element element = doc.GetElement(elementId);
                if (element == null) continue;

                // Obtain the geometry
                Options options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = false
                };

                GeometryElement geometryElement = element.get_Geometry(options);
                if (geometryElement == null) continue;

                //Process the geometry with the method written below
                foreach (GeometryObject geoObject in geometryElement)
                {
                    if (geoObject is GeometryInstance geometryInstance)
                    {
                        GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
                        if (instanceGeometry == null) continue;

                        foreach (GeometryObject instanceGeoObject in instanceGeometry)
                        {
                            ProcessGeometryObject(instanceGeoObject, surfaceSelector, lateralFaces);
                        }
                    }
                    else
                    {
                        ProcessGeometryObject(geoObject, surfaceSelector, lateralFaces);
                    }
                }

                TaskDialog.Show("Lateral Faces", $"Found {lateralFaces.Count} lateral faces.");
            }
            return Result.Succeeded;
        }

            
        

        //Method for getting the lateral faces of the element givent its geometry, the surfaceSelector and the lateralFacesList
        private static void ProcessGeometryObject(GeometryObject geoObject, SurfaceSelector surfaceSelector, List<PlanarFace> lateralFaces)
        {
            if (geoObject is Solid solid)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace planarFace)
                    {
                        // Create a reference
                        Reference faceReference = planarFace.Reference;
                        XYZ facePosition = planarFace.Origin;

                        // Is it lateral?
                        if (surfaceSelector.AllowReference(faceReference, facePosition))
                        {
                            lateralFaces.Add(planarFace);
                        }
                    }
                }
            }
        }
    }
}
