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
                        if (includeDimensionWidths)
                        {


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


        //Method for putting all the dimensions for the walls, stairs, columns, beams and floors. Include wall widths if necessary
        public void DimensionElements(bool includeDimensionWidth)
        {
            // List of lines
            List<EdgeArray> elementEdges = new List<EdgeArray>();

			// Get all the elements filters
			ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
			ElementCategoryFilter stairFilter = new ElementCategoryFilter(BuiltInCategory.OST_Stairs);
			ElementCategoryFilter columnFilter = new ElementCategoryFilter(BuiltInCategory.OST_Columns);
			ElementCategoryFilter structuralColumnFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);
			ElementCategoryFilter beamFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
			ElementCategoryFilter beamFilter2 = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingOther);
			ElementCategoryFilter beamFilter3 = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFramingSystem);
			ElementCategoryFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

			// Combine the filters using a LogicalOrFilter
			LogicalOrFilter combinedFilter = new LogicalOrFilter(new List<ElementFilter> {
                wallFilter,
                stairFilter,
                columnFilter,
                structuralColumnFilter,
                beamFilter,
				beamFilter2,
				beamFilter3,
				floorFilter
            });

            // Get the element IDs
            List<ElementId> elementIds = new FilteredElementCollector(_document, _activeView.Id)
                .WherePasses(combinedFilter)
                .WhereElementIsNotElementType()
                .ToElementIds()
                .ToList();

            //Get the reference planes
            ReferencePlane referencePlane = new FilteredElementCollector(_document, _activeView.Id)
            .OfClass(typeof(ReferencePlane))
            .WhereElementIsNotElementType()
            .Cast<ReferencePlane>()
            .FirstOrDefault();

            //Get the reference face
            PlanarFace referenceFace = GetPlanarFaceFromReferencePlane(referencePlane);


            // Surface selector
            List<string> selectedSuperiorSurfaces = new List<string> { "Laterales"};
            SurfaceSelector superiorSurfaceSelector = new SurfaceSelector(selectedSuperiorSurfaces, _document);

            // Options for geometry extraction
            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false
            };

            int solidCount = 0;
            int superiorFaceCount = 0;

            // Iterate each element and get the superior surfaces
            foreach (ElementId elementId in elementIds)
            {
                Element element = _document.GetElement(elementId);
                GeometryElement geometry = element.get_Geometry(options);

                if (geometry == null) continue;

                foreach (var geomObj in geometry)
                {
                    if (geomObj is Solid solid && solid.Volume > 0)
                    {
                        solidCount++;

                        foreach (Face face in solid.Faces)
                        {
                            superiorFaceCount++;

                            if (face is PlanarFace planarFace && planarFace.Reference != null &&
                                superiorSurfaceSelector.AllowReference(planarFace.Reference, planarFace.Origin))
                            {
    

                                    elementEdges.Add(face.EdgeLoops.get_Item(0));
								
                            }
                        }
                    }
                }
            }

            Dictionary<Reference, XYZ> curveIntersectionsDictionary = new Dictionary<Reference, XYZ>();

			foreach (EdgeArray edges in elementEdges)
            {              
                // Verificar si la curva intersecta el plano
                IntersectionResultArray intersectionResults;

                foreach(Edge edge in edges)
                {
					SetComparisonResult result = referenceFace.Intersect(edge.AsCurve(), out intersectionResults);
					// Si hay intersección, agregar la curva a la lista
					if (result == SetComparisonResult.Overlap && intersectionResults != null && intersectionResults.Size > 0)
					{
						foreach (IntersectionResult res in intersectionResults)
						{
                            curveIntersectionsDictionary.Add(edge.Reference, res.XYZPoint);
						}
						break;
					}
				}
            }

			List<KeyValuePair<Reference, XYZ>> sortedIntersections = curveIntersectionsDictionary
				.OrderBy(p => p.Value.X)
				.ThenBy(p => p.Value.Y)
				.ToList();
			Level level = _document.GetElement(_activeView.GenLevel.Id) as Level;
			double elevation = level.Elevation;

			if (sortedIntersections.Count < 2)
			{
				TaskDialog.Show("Error", "No hay suficientes intersecciones para crear dimensiones.");
				return;
			}



			using (Transaction trans = new Transaction(_document, "Create Dimensions"))
			{
				trans.Start();

				// Obtener la vista activa donde se colocarán las dimensiones
				View view = _document.ActiveView;

				// Crear las dimensiones entre cada par de puntos consecutivos
				for (int i = 0; i < sortedIntersections.Count - 1; i++)
				{
                    try
                    {
                        XYZ point1 = new XYZ(sortedIntersections[i].Value.X, sortedIntersections[i].Value.Y, elevation);
                        XYZ point2 = new XYZ(sortedIntersections[i + 1].Value.X, sortedIntersections[i + 1].Value.Y, elevation);

                        // Crear una línea de referencia entre los dos puntos
                        Line dimensionLine = Line.CreateBound(point1, point2);

                        // Obtener referencias de las curvas
                        Reference ref1 = sortedIntersections[i].Key;
                        Reference ref2 = sortedIntersections[i + 1].Key;

                        if (ref1 != null && ref2 != null)
                        {
                            ReferenceArray references = new ReferenceArray();
                            references.Append(ref1);
                            references.Append(ref2);

                            // Crear la dimensión en la vista actual
                            Dimension dimension = _document.Create.NewDimension(view, dimensionLine, references);
                        }
                    }
                    catch (Exception ex) { }

				}

				trans.Commit();
			}



			// Show the results in a TaskDialog
			TaskDialog.Show("Resultado del Filtro",
                $"Se encontraron {elementIds.Count} elementos que pertenecen a las categorías seleccionadas:\n" +
                "- Muros (Walls)\n" +
                "- Escaleras (Stairs)\n" +
                "- Columnas (Columns)\n" +
                "- Columnas Estructurales (Structural Columns)\n" +
                "- Vigas (Structural Framing)\n" +
                "- Pisos (Floors)\n" +
                $"Se encontraron {solidCount} sólidos, {superiorFaceCount} caras superiores y {sortedIntersections.Count} curvas que intersectan.");
        }

        private XYZ GetOffsetByWallOrientation(XYZ point, XYZ orientation, int value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }



        // Method for generating a planar face given a reference plane
        private PlanarFace GetPlanarFaceFromReferencePlane(ReferencePlane referencePlane)
        {
            XYZ origin = referencePlane.BubbleEnd;
            XYZ end = referencePlane.FreeEnd;
            XYZ normal = referencePlane.Normal; // Normal vector

            // Get the direction of the ReferencePlane
            XYZ direction = (end - origin).Normalize();

            // Get the actual length of the ReferencePlane
            double length = origin.DistanceTo(end);

            // Assume a standard height (Revit does not directly provide a "height" for the ReferencePlane)
            double height = 200; // You can adjust this according to the project's scale

            // Calculate the points of a rectangle on the ReferencePlane
            XYZ p1 = origin + new XYZ(0, 0, height);
            XYZ p2 = origin + new XYZ(0, 0, 0);
            XYZ p3 = p2 + length * direction;
            XYZ p4 = p1 + length * direction;

            // Create a rectangular profile
            CurveLoop profile = new CurveLoop();
            profile.Append(Line.CreateBound(p1, p2));
            profile.Append(Line.CreateBound(p2, p3));
            profile.Append(Line.CreateBound(p3, p4));
            profile.Append(Line.CreateBound(p4, p1));

            // Create an extruded solid with a small depth
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { profile }, normal, 0.1);

            // Get the face that matches the ReferencePlane
            foreach (Face face in solid.Faces)
            {
                if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(normal))
                {
					using (Transaction trans = new Transaction(_document, "Create DirectShape"))
					{
						trans.Start();

						// Crear un tipo de categoría para el DirectShape
						Category category = _document.Settings.Categories.get_Item(BuiltInCategory.OST_GenericModel);
						ElementId categoryId = category.Id;


						// Crear un DirectShape con la geometría
						DirectShape ds = DirectShape.CreateElement(_document, categoryId);
						ds.SetShape(new List<GeometryObject> { solid });

						trans.Commit();
					}
					return planarFace;
                }
            }

            return null;
        }



	}
}
