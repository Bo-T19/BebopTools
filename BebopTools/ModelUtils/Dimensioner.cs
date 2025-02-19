using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BebopTools.SelectionUtils;

namespace BebopTools.ModelUtils
{
    internal class Dimensioner
    {
        //The document and the active view are required for this class to work
        private Document _document;
        private View _activeView;

        //Constructor
        public Dimensioner(Document document, View activeView)
        {
            _activeView = activeView;
           _document = document;
        }

        //Method for putting all the dimensions for the walls in a floor plan, it asks the user if 
        //dimensioning the widths of the walls is necessary
        public void DimensionFloorPlanWalls(bool includeDimensionWidths, List<ElementId> wallElements)
        {
            // Get the active view and ensure it's a floor plan. Create the surfaces selector
            List<string> selectedLateralSurfaces = new List<string> { "Laterales" };
            SurfaceSelector lateralSurfaceSelector = new SurfaceSelector(selectedLateralSurfaces, _document);
            List<string> selectedSuperiorSurfaces = new List<string> { "Superiores", "Inferiores" };
            SurfaceSelector superiorSurfaceSelector = new SurfaceSelector(selectedSuperiorSurfaces, _document);


            if (_activeView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Error", "This only works in a floor plan");
                return;
            }


            foreach (ElementId wallElement in wallElements)
            {
                using (Transaction t = new Transaction(_document))
                {
                    t.Start("Create new dimensions");

                    try
                    {
                        //Get the wall and its relevant elements
                        Wall wall = _document.GetElement(wallElement) as Wall;
                        if (wall == null || wall.Orientation.IsAlmostEqualTo(XYZ.Zero)) continue;
                        XYZ wallOrientation = wall.Orientation;
                        PlanarFace wallPrincipalFace = null;
                        LocationCurve wallLoc = wall.Location as LocationCurve;
                        Line wallLine = wallLoc.Curve as Line;

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

                        Reference r1 = null, r2 = null;

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
                        if (includeDimensionWidths) {

                            
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



                            r1 = topTwoLongestEdges[0].Reference;
                            r2 = topTwoLongestEdges[1].Reference;

                            ReferenceArray wallWidthReferenceArray = new ReferenceArray();
                            wallWidthReferenceArray.Append(r1);
                            wallWidthReferenceArray.Append(r2);




                            XYZ point1 = shortestEdge.AsCurve().GetEndPoint(0);
                            XYZ point2 = shortestEdge.AsCurve().GetEndPoint(1);

                            Line dimLine = Line.CreateBound(point1, point2);




                            Dimension newDim = _document.Create.NewDimension(_document.ActiveView, dimLine, wallWidthReferenceArray);
                            if (newDim.Value < 0.0328084)
                            {
                                // If the value is less than 1 cm, deletes the dimension
                                _document.Delete(newDim.Id);
                            }

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
                            FamilyInstance curFI = _document.GetElement(elemId) as FamilyInstance;
                            Reference curRef = GetSpecialFamilyReference(curFI, SpecialReferenceType.Left);
                            Reference curRef2 = GetSpecialFamilyReference(curFI, SpecialReferenceType.Right);
                            wallLengthReferenceArray.Append(curRef);
                            wallLengthReferenceArray.Append(curRef2);
                        }

                        wallLengthReferenceArray.Append(r2);

                        // create dimension line

                        XYZ offset1 = GetOffsetByWallOrientation(wallLine.GetEndPoint(0), wall.Orientation, -2);
                        XYZ offset2 = GetOffsetByWallOrientation(wallLine.GetEndPoint(1), wall.Orientation, -2);

                        Line dimLine2 = Line.CreateBound(offset1, offset2);

                        Dimension newDim2 = _document.Create.NewDimension(_activeView, dimLine2, wallLengthReferenceArray);
                        if (newDim2.Value < 0.0328084)
                        {
                            // If the value is less than 1 cm, deletes the dimension
                            _document.Delete(newDim2.Id);
                        }

                        t.Commit();
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
            }
        }


        //Helper methods for the dimensioning tools, sources: Jeremmy Tammik and Michael KilKelly
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
