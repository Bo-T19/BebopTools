using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace BebopTools.SelectionUtils
{
    public class ElementsSelector
    {
        private Document _doc;
        private Categories _categories;
        private List<Category> _modelCategories = new List<Category>();

        //Constructror which takes the document and filters just the model categories
        public ElementsSelector(Document doc)
        {
            _doc = doc;
            _categories = doc.Settings.Categories;
            foreach (Category category in _categories)
            {
                if (category.CategoryType == CategoryType.Model)
                {
                    _modelCategories.Add(category);
                }
            }
        }

        //Method for getting all the model elements Ids in the document

        public IEnumerable<ElementId> GetModelElementsInstances()
        {
            List<ElementId> modelElementsIds = new List<ElementId>();

            foreach (Category category in _modelCategories)
            {
                var allElements = new FilteredElementCollector(_doc)
                                      .OfCategoryId(category.Id)
                                      .WhereElementIsNotElementType()
                                      .ToElementIds();

                modelElementsIds.AddRange(allElements);
            }

            return modelElementsIds;
        }

        //Method for getting all the elements ids in the active view

        public IEnumerable<ElementId> AllElementsInActiveView()
        {

            return new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                                      .WhereElementIsNotElementType()
                                      .ToElementIds();
        }

        //Method for getting al the levels in the project
        public List<string> AllLevelsInProject()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(level => level.Name)
                .ToList();
        }

        //Method for getting a dicionary with the level Id and a the value of the elevation and the name. The dictionary is Sorted from the smallest elevation to the highest
        public List<KeyValuePair<ElementId, (double Elevation, string Name)>> GetLevelsAndElevationsOrdered()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToDictionary(
                    level => level.Id,
                    level => (level.Elevation, level.Name)
                )
                .OrderBy(pair => pair.Value.Elevation)
                .ToList();
        }
    }
}
