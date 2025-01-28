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

            // Get the active view and ensure it's a floor plan. Create the surfaces selector
            List<string> selectedLateralSurfaces = new List<string> { "Laterales" };
            SurfaceSelector lateralSurfaceSelector = new SurfaceSelector(selectedLateralSurfaces, doc);
            List<string> selectedSuperiorSurfaces = new List<string> { "Superiores", "Inferiores" };
            SurfaceSelector superiorSurfaceSelector = new SurfaceSelector(selectedSuperiorSurfaces, doc);

            View activeView = doc.ActiveView;

            if (activeView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Error", "This only works in a floor plan");
                return Result.Succeeded;
            }

            // Get all the walls in the view
            List<ElementId> wallElements = new FilteredElementCollector(doc, activeView.Id)
                                            .OfCategory(BuiltInCategory.OST_Walls)
                                            .WhereElementIsNotElementType()
                                            .ToElementIds()
                                            .ToList();

            // Iterate through all the walls
            foreach (ElementId wallElement in wallElements)
            {
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Create new dimensions");

                    try
                    {
                        Wall wall = doc.GetElement(wallElement) as Wall;
                        if (wall == null || wall.Orientation.IsAlmostEqualTo(XYZ.Zero)) continue;

                        XYZ wallOrientation = wall.Orientation;
                        PlanarFace wallPrincipalFace = null;

                        Options options = new Options
                        {
                            ComputeReferences = true,
                            IncludeNonVisibleObjects = false
                        };

                        List<Solid> solids = wall.get_Geometry(options)
                                                 .Select(g => g as Solid)
                                                 .Where(s => s != null && s.Volume > 0)
                                                 .ToList();

                        if (solids.Count == 0) continue;

                        // Get the lateral faces of the wall and the superior faces

                        List<PlanarFace> wallSuperiorFaces = new List<PlanarFace>();


                        foreach (Solid solid in solids)
                        {
                            FaceArray faces = solid.Faces;
                            foreach (Face face in faces)
                            {
                                if (face is PlanarFace planarFace)
                                {
                                    if (planarFace.Reference != null &&
                                        superiorSurfaceSelector.AllowReference(planarFace.Reference, planarFace.Origin))
                                    {
                                        wallSuperiorFaces.Add(planarFace);
                                    }
                                    else if (planarFace.FaceNormal.IsAlmostEqualTo(wallOrientation))
                                    {
                                        wallPrincipalFace = planarFace;
                                    }
                                }    
                            }
                        }


                        //Create the dimension line for the width of the wall, this is my own development
                        PlanarFace firstSuperiorFace = wallSuperiorFaces[0];
                        EdgeArrayArray firstSuperiorFaceEdgeArrays = firstSuperiorFace.EdgeLoops;
                        EdgeArray firstSuperiorFaceEdges = firstSuperiorFaceEdgeArrays.get_Item(0);

                        List<Edge> firstSuperiorFaceEdgesList = new List<Edge>();

                        foreach (Edge edge in firstSuperiorFaceEdges)
                        {
                            firstSuperiorFaceEdgesList.Add(edge);
                        }

                        List<Edge> topTwoLongestEdges = firstSuperiorFaceEdgesList
                                                        .OrderByDescending(edge => edge.ApproximateLength)
                                                        .Take(2)
                                                        .ToList();
                        Edge shortestEdge = firstSuperiorFaceEdgesList
                                    .OrderBy(edge => edge.ApproximateLength)
                                    .FirstOrDefault();


                        Reference r1 = null, r2 = null;
                        r1 = topTwoLongestEdges[0].Reference;
                        r2 = topTwoLongestEdges[1].Reference;

                        ReferenceArray wallWidthReferenceArray = new ReferenceArray();
                        wallWidthReferenceArray.Append(r1);
                        wallWidthReferenceArray.Append(r2);


                        LocationCurve wallLoc = wall.Location as LocationCurve;
                        Line wallLine = wallLoc.Curve as Line;

                        XYZ point1 = shortestEdge.AsCurve().GetEndPoint(0);
                        XYZ point2 = shortestEdge.AsCurve().GetEndPoint(1);

                        Line dimLine = Line.CreateBound(point1, point2);




                        Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, wallWidthReferenceArray);
                        if (newDim.Value < 0.0328084)
                        {
                            // If the value is less than 1 cm, deletes the dimension
                            doc.Delete(newDim.Id);
                        }

                        //Create the dimensions along the wall and the elements it contains, this was based on the code
                        //written by Michael Kilkelly: https://github.com/kilkellym/RAA_HowTo_DimensionElements/blob/master/RAA_HowTo_DimensionElements/Command1.cs
                        ReferenceArray wallLengthReferenceArray = new ReferenceArray();
                        EdgeArrayArray edgeArrays = wallPrincipalFace.EdgeLoops;
                        EdgeArray edges = edgeArrays.get_Item(0);

                        List<Edge> edgeList = new List<Edge>();
                        foreach (Edge edge in edges)
                        {
                            Line line = edge.AsCurve() as Line;

                            if (IsLineVertical(line) == true)
                            {
                                edgeList.Add(edge);
                            }
                        }

                        List<Edge> sortedEdges = edgeList.OrderByDescending(e => e.AsCurve().Length).ToList();
                        r1 = sortedEdges[0].Reference;
                        r2 = sortedEdges[1].Reference;

                        wallLengthReferenceArray.Append(r1);


                        List<BuiltInCategory> catList = new List<BuiltInCategory>() { BuiltInCategory.OST_Windows, BuiltInCategory.OST_Doors };
                        ElementMulticategoryFilter wallFilter = new ElementMulticategoryFilter(catList);

                        // get windows and doors from wall and create reference
                        List<ElementId> wallElemsIds = wall.GetDependentElements(wallFilter).ToList();

                        foreach (ElementId elemId in wallElemsIds)
                        {
                            FamilyInstance curFI = doc.GetElement(elemId) as FamilyInstance;
                            Reference curRef = GetSpecialFamilyReference(curFI, SpecialReferenceType.Left);
                            Reference curRef2 = GetSpecialFamilyReference(curFI, SpecialReferenceType.Right);
                            wallLengthReferenceArray.Append(curRef);
                            wallLengthReferenceArray.Append(curRef2);
                        }

                        wallLengthReferenceArray.Append(r2);

                        // create dimension line

                        XYZ offset1 = GetOffsetByWallOrientation(wallLine.GetEndPoint(0), wall.Orientation, 1);
                        XYZ offset2 = GetOffsetByWallOrientation(wallLine.GetEndPoint(1), wall.Orientation, 1);

                        Line dimLine2 = Line.CreateBound(offset1, offset2);

                        Dimension newDim2 = doc.Create.NewDimension(activeView, dimLine2, wallLengthReferenceArray);
                        if (newDim2.Value < 0.0328084)
                        {
                            // If the value is less than 1 cm, deletes the dimension
                            doc.Delete(newDim2.Id);
                        }

                        t.Commit();
                    }



                    catch (Exception ex)
                    {
                        continue;
                    }

                }
            }
            return Result.Succeeded;
        }

        public enum SpecialReferenceType
        {
            Left = 0,
            CenterLR = 1,
            Right = 2,
            Front = 3,
            CenterFB = 4,
            Back = 5,
            Bottom = 6,
            CenterElevation = 7,
            Top = 8
        }
        private bool IsLineVertical(Line line)
        {
            if (line.Direction.IsAlmostEqualTo(XYZ.BasisZ) || line.Direction.IsAlmostEqualTo(-XYZ.BasisZ))
                return true;
            else
                return false;
        }

        private Reference GetSpecialFamilyReference(FamilyInstance inst, SpecialReferenceType refType)
        {
            // source for this method https://thebuildingcoder.typepad.com/blog/2016/04/stable-reference-string-magic-voodoo.html

            Reference indexRef = null;

            int idx = (int)refType;

            if (inst != null)
            {
                Document dbDoc = inst.Document;

                Options geomOptions = new Options();
                geomOptions.ComputeReferences = true;
                geomOptions.DetailLevel = ViewDetailLevel.Undefined;
                geomOptions.IncludeNonVisibleObjects = true;

                GeometryElement gElement = inst.get_Geometry(geomOptions);
                GeometryInstance gInst = gElement.First() as GeometryInstance;

                String sampleStableRef = null;

                if (gInst != null)
                {
                    GeometryElement gSymbol = gInst.GetSymbolGeometry();

                    if (gSymbol != null)
                    {
                        foreach (GeometryObject geomObj in gSymbol)
                        {
                            if (geomObj is Solid)
                            {
                                Solid solid = geomObj as Solid;

                                if (solid.Faces.Size > 0)
                                {
                                    Face face = solid.Faces.get_Item(0);
                                    sampleStableRef = face.Reference.ConvertToStableRepresentation(dbDoc);
                                    break;
                                }
                            }
                            else if (geomObj is Curve)
                            {
                                Curve curve = geomObj as Curve;
                                Reference curveRef = curve.Reference;
                                if (curveRef != null)
                                {
                                    sampleStableRef = curve.Reference.ConvertToStableRepresentation(dbDoc);
                                    break;
                                }

                            }
                            else if (geomObj is Point)
                            {
                                Point point = geomObj as Point;
                                sampleStableRef = point.Reference.ConvertToStableRepresentation(dbDoc);
                                break;
                            }
                        }
                    }

                    if (sampleStableRef != null)
                    {
                        String[] refTokens = sampleStableRef.Split(new char[] { ':' });

                        String customStableRef = refTokens[0] + ":"
                          + refTokens[1] + ":" + refTokens[2] + ":"
                          + refTokens[3] + ":" + idx.ToString();

                        indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);

                        GeometryObject geoObj = inst.GetGeometryObjectFromReference(indexRef);

                        if (geoObj != null)
                        {
                            String finalToken = "";
                            if (geoObj is Edge)
                            {
                                finalToken = ":LINEAR";
                            }

                            if (geoObj is Face)
                            {
                                finalToken = ":SURFACE";
                            }

                            customStableRef += finalToken;
                            indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);
                        }
                        else
                        {
                            indexRef = null;
                        }
                    }
                }
                else
                {
                    throw new Exception("No Symbol Geometry found...");
                }
            }
            return indexRef;
        }
        private XYZ GetOffsetByWallOrientation(XYZ point, XYZ orientation, int value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }
    }
}
