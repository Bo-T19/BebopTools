using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace BebopTools.SelectionUtils
{
    internal class SelectorFromCategory : ISelectionFilter
    {
        BuiltInCategory _category;
        private Document _doc;

        public SelectorFromCategory(Document doc,BuiltInCategory category)
        {
            _doc = doc;
            _category = category;   
        }
        bool ISelectionFilter.AllowElement(Element elem)
        {
            return elem.Category != null && elem.Category.BuiltInCategory == _category;
        }

        bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
