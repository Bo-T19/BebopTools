using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace BebopTools.SelectionUtils
{
    internal class ElementsSelector
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

        //Method for getting all the model elements levels Ids

    }
}
