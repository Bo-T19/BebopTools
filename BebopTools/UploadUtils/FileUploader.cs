using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BebopTools.UploadUtils
{
    internal class FileUploader
    {
        private string _filePath;


        //Constructor that requires a filePath 
        public FileUploader(string filePath)
        {
            _filePath = filePath;

        }

        //Method for getting the dictionary
        public Dictionary<string,string> GetInfo()
        {
            List < List<string> > resultList = new List<List<string>>();

            //Uploading the excel data to the program, creating a list of string pairs
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(_filePath, false))
            {

                WorkbookPart workbookPart = document.WorkbookPart ?? document.AddWorkbookPart();
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                int rowIndex = 0;

                foreach (Row r in sheetData.Elements<Row>())
                {
                    rowIndex++;

                    if (rowIndex < 2)
                    {
                        continue;  // Start at row 2
                    }

                    List<string> items = new List<string>();
                    foreach (Cell cell in r.Descendants<Cell>())
                    {
                        string cellValue = cell.CellValue?.InnerText;

                        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                        {
                            int sharedStringIndex = int.Parse(cellValue);
                            SharedStringTable sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                            cellValue = sharedStringTable.Elements<SharedStringItem>().ElementAt(sharedStringIndex).InnerText;
                        }
                        items.Add(cellValue);
                    }

                    if (items.Count >= 2)  
                    {
                        resultList.Add(items);
                    }

                }

            }

            //Convert the list to a dictionary
            Dictionary<string, string> resultDictionary = new Dictionary<string, string>();

            foreach (var item in resultList)
            {
                string key = item[0];
                string value = item[1];

                if (!resultDictionary.ContainsKey(key))
                {
                    resultDictionary[key] = value; 
                }

            }
            return resultDictionary;
        }

    }
}
