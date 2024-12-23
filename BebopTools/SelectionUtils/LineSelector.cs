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
    internal class LineSelector : ISelectionFilter
    {
        private Document _doc;
        public LineSelector(Document doc)
        {
            _doc = doc;
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Lines;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

}
