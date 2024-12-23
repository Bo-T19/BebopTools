using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ParameterUtils;
using BebopTools.SelectionUtils;
using BebopTools.DownloadAndUploadUtils;
using BebopTools.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BebopTools.SelectionUtils
{
    internal class VolumeSelector: ISelectionFilter
    {
        private Document _doc;

        public VolumeSelector(Document doc)
        {
            _doc = doc;
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category.CategoryType == CategoryType.Model;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
