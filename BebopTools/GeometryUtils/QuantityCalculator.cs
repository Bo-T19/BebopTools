using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2013.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BebopTools.GeometryUtils
{
    internal class QuantityCalculator
    {
        private Document _doc;
        public QuantityCalculator(Document doc)
        {
            _doc = doc;
        }
        public double VolumeCalculator(IList<Reference> references)
        {
            double total = 0;

            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            foreach (Reference reference in references)
            {
                Element element = _doc.GetElement(reference.ElementId);
                GeometryElement geometryElement = element.get_Geometry(options);


                foreach (GeometryObject gObj in geometryElement)
                {
                    Solid geoSolid = gObj as Solid;
                    if (geoSolid != null)
                    {
                        total += geoSolid.Volume;
                    }
                    else if (gObj is GeometryInstance)
                    {
                        GeometryInstance geoInst = gObj as GeometryInstance;
                        GeometryElement geoElem = geoInst.SymbolGeometry;
                        foreach (GeometryObject gObjInstance in geoElem)
                        {
                            Solid geoSolid2 = gObjInstance as Solid;
                            if (geoSolid2 != null)
                            {
                                total += geoSolid2.Volume;
                            }
                        }
                    }
                }

            }
            return UnitConversor.ConvertCubicFeetToCubicMeters(total);
        }

        public double AreaCalculator(IList<Reference> references)
        {
            double total = 0;
            foreach (Reference reference in references)
            {
                Element element = _doc.GetElement(reference);
                GeometryObject geometryObject = element.GetGeometryObjectFromReference(reference);

                if (geometryObject is PlanarFace face && face.Area > 0)
                {
                    total += UnitConversor.ConvertSquareFeetToSquareMeters(face.Area);
                }

            }
            return total;
        }



        public double LengthCalculator(IList<Reference> references)
        {
            double total = 0;
            foreach (Reference reference in references)
            {
                Element element = _doc.GetElement(reference);
                LocationCurve locationCurve = element.Location as LocationCurve;

                if (locationCurve != null)
                {
                    total += locationCurve.Curve.Length;
                }

            }
            return UnitConversor.ConvertFeetToMeters(total);
        }
    }
}
