using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopTools.SelectionUtils
{
    //This class will allow the selection of elements surfaces, the direction of the faces (superior, inferior, lateral) must be specified
    internal class SurfaceSelector : ISelectionFilter
    {
        private HashSet<string> _selectedSurfaces;
        private Document _doc;
        public SurfaceSelector(List<string> selectedSurfaces, Document doc)
        {
            _selectedSurfaces = new HashSet<string> (selectedSurfaces);
            _doc = doc;
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category.CategoryType == CategoryType.Model;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {

            var element = _doc.GetElement(reference.ElementId);
            var geometryObject = element.GetGeometryObjectFromReference(reference) as PlanarFace;

            bool superiorFaces = _selectedSurfaces.Contains("Superiores") && (geometryObject.FaceNormal.Z == 1);
            bool inferiorFaces = _selectedSurfaces.Contains("Inferiores") && (geometryObject.FaceNormal.Z == -1);
            bool lateralFaces = _selectedSurfaces.Contains("Laterales") && (geometryObject.FaceNormal.Z != 1 && geometryObject.FaceNormal.Z != -1 && Math.Sqrt((Math.Pow((geometryObject.FaceNormal.X),2) + Math.Pow((geometryObject.FaceNormal.Y),2)))==1);

            return superiorFaces || inferiorFaces || lateralFaces;
        }
    }
}
