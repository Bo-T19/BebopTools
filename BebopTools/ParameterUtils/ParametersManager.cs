using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BebopTools.ParameterUtils
{
    public class ParametersManager
    {
        private Document _document;
        public ParametersManager(Document document)
        {
            _document = document;
        }

        //Method for getting a list with strings of the project parameters names in the Revit Document
        public List<String> GetProjectParametersNames()
        {

            List<string> projectParametersNames = new List<string>();

            // Get the binding map and its iterator
            BindingMap bindingMap = _document.ParameterBindings;
            DefinitionBindingMapIterator iterator = bindingMap.ForwardIterator();
            iterator.Reset();

            //This iterator thing is strange and I should undertand it better
            while (iterator.MoveNext())
            {
                Definition definition = iterator.Key as Definition;
                if (definition != null)
                {
                    projectParametersNames.Add(definition.Name); // Add the parameter name
                }
            }

            return projectParametersNames;

        }

        //Method for filling the parameter data given a Dictionary of Ids, Values
        public void FillParameters(Dictionary<string, string> dictionary, String parameter)
        {
            List<string> IdsFailures = new List<string>();
            using (var transaction = new Transaction(_document, "Parametrizacion"))
            {


                transaction.Start();
                foreach (var element in dictionary)
                {
                    try
                    {
                        int.TryParse(element.Key, out int elementIdNumber);

                        if (elementIdNumber == 0)
                        {
                            _document.GetElement(element.Key)
                                     .LookupParameter(parameter)
                                     .Set(element.Value);
                        }
                        else
                        {
                            _document.GetElement(new ElementId(int.Parse(element.Key)))
                                     .LookupParameter(parameter)
                                     .Set(element.Value);
                        }

                    }
                    catch (Exception e)
                    {
                        IdsFailures.Add(element.Key);
                    }

                }

                if (IdsFailures.Count > 0)
                {
                    TaskDialog.Show("The following elements couldn't be found", string.Join(", ", IdsFailures));
                }

                transaction.Commit();

            }
        }

        //Method for filling a parameter given the information of another parameter and using a dictionary that maps them
        public void AssociateParameters(IEnumerable<ElementId> elementIds, Dictionary<string, string> parameterDictionary, string sourceParameter, string targetParameter)
        {
            using (var transaction = new Transaction(_document, "Parametrizacion"))
            {

                string valueFound;

                transaction.Start();

                foreach (ElementId elementId in elementIds)
                {

                    Element element = _document.GetElement(elementId);
                    try
                    {
                        Parameter parameter = element.LookupParameter(sourceParameter);
                        string sourceParameterValue = element.LookupParameter(sourceParameter).AsString();
                        parameterDictionary.TryGetValue(element.LookupParameter(sourceParameter).AsString(), out string value);
                        valueFound = value.Trim();
                        element.LookupParameter(targetParameter).Set(valueFound);

                    }
                    catch (Exception e)
                    {
                        valueFound = "Not found";
                    }


                }

                transaction.Commit();


            }

        }

        //Get the element's elevation based on its bounding box

        public double GetElementElevation(Element element)
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(_document.ActiveView);
            double maxElevation = boundingBox.Max.Z;
            double minElevation = boundingBox.Min.Z;

            return (maxElevation + minElevation)/2;
        }

        //Get the level of an element
        public Level GetElementLevel(Element element)
        {
            List<string> possibleLevelParams = new List<string> { "Reference Level", "Base Level", "Schedule Level", "Level" };
            ElementId levelId = element.LevelId;
            Level elementLevel = _document.GetElement(levelId) as Level;

            if (elementLevel != null)
            {
                return elementLevel;
            }
            else
            {
                foreach (var paramName in possibleLevelParams)
                {
                    var parameter = element.LookupParameter(paramName);

                    if (parameter != null)
                    {
                        levelId = parameter.AsElementId();
                        if (levelId != ElementId.InvalidElementId)
                        {
                            var level = _document.GetElement(levelId) as Level;
                            if (level != null)
                                return level;
                        }
                    }
                }
                return null;
            }

        }


        
        //Level associators

        public void AssociateLevels(IEnumerable<ElementId> elementIds,
                                    List<KeyValuePair<ElementId, (double Elevation, string Name)>>  levelsDictionary, 
                                    string parameterName,
                                    List<string> acceptedLevels,
                                    string associateOption)
        {

            //Prepare the accepted levels
            var acceptedLevelsSet = new HashSet<string>(acceptedLevels);
            var filteredLevelsDictionary = levelsDictionary
                                            .Where(pair => acceptedLevelsSet.Contains(pair.Value.Name))
                                            .OrderByDescending(pair => pair.Value.Elevation) 
                                            .ToList();
            //Prepare the levels for faster search processes
            var lowestLevel = filteredLevelsDictionary.LastOrDefault();
            var levelElevations = filteredLevelsDictionary
                                    .ToDictionary(pair => pair.Key, pair => pair.Value);


            using (var transaction = new Transaction(_document, "Parametrizacion"))
            {
                transaction.Start();



                if (associateOption == "Niveles ya asignados (Recomendado para ARQ y EST)")
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        try
                        {
                            Element element = _document.GetElement(elementId);
                            Level elementLevel = this.GetElementLevel(element);



                            if (elementLevel == null)
                            {
                                double elementElevation = this.GetElementElevation(element);

                                var nearestLevel = filteredLevelsDictionary
                                                    .Where(pair => pair.Value.Elevation <= elementElevation)
                                                    .OrderByDescending(pair => pair.Value.Elevation)
                                                    .FirstOrDefault();

                                // Assign the level if it exists
                                if (!nearestLevel.Equals(default(KeyValuePair<ElementId, (double, string)>)))
                                {
                                    element.LookupParameter(parameterName)?.Set(nearestLevel.Value.Name);
                                }
                                else
                                {
                                    element.LookupParameter(parameterName)?.Set(lowestLevel.Value.Name);
                                }
                                continue;
                            }



                            if (acceptedLevelsSet.Contains(elementLevel.Name))
                            {
                                // If there's a coincidence, assign the level
                                element.LookupParameter(parameterName)?.Set(elementLevel.Name);
                            }
                            else
                            {
                                // Search for the level with the closest elevation 
                                var nearestLevel = filteredLevelsDictionary
                                    .Where(pair => pair.Value.Elevation <= elementLevel.Elevation)
                                    .OrderByDescending(pair => pair.Value.Elevation)
                                    .FirstOrDefault();

                                // Assign the level if it exists
                                if (!nearestLevel.Equals(default(KeyValuePair<ElementId, (double, string)>)))
                                {
                                    element.LookupParameter(parameterName)?.Set(nearestLevel.Value.Name);
                                }
                                else
                                {
                                    // Assign the first level if there are no levels above the one we are looking for
                                    element.LookupParameter(parameterName)?.Set(lowestLevel.Value.Name);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                }
                else
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        try
                        {
                            Element element = _document.GetElement(elementId);
                            double elementElevation = this.GetElementElevation(element);

                            var nearestLevel = filteredLevelsDictionary
                                                .Where(pair => pair.Value.Elevation <= elementElevation)
                                                .OrderByDescending(pair => pair.Value.Elevation)
                                                .FirstOrDefault();

                                // Assign the level if it exists
                                if (!nearestLevel.Equals(default(KeyValuePair<ElementId, (double, string)>)))
                                {
                                    element.LookupParameter(parameterName)?.Set(nearestLevel.Value.Name);
                                }
                                else
                                {
                                    element.LookupParameter(parameterName)?.Set(lowestLevel.Value.Name);
                                }
                                continue;                            

                        }
                        catch (Exception ex)
                        {
                        }

                    }
                }
                    transaction.Commit();
            }
        }
    }
}
