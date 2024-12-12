using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string AssociateParameters(ElementId elementId, Dictionary<string, string> parameterDictionary, string sourceParameter, string targetParameter)
        {
            using (var transaction = new Transaction(_document, "Parametrizacion"))
            {

                string valueFound;
                    
                transaction.Start();

                Element element = _document.GetElement(elementId);
                try
                {
                    parameterDictionary.TryGetValue(element.LookupParameter(sourceParameter).AsString(), out string value);
                    valueFound = value.Trim();
                    element.LookupParameter(targetParameter).Set(valueFound);

                }
                catch (Exception e)
                {
                    valueFound = "Not found";
                }

                transaction.Commit();

                return valueFound;
            }
        
        }
    }
}
