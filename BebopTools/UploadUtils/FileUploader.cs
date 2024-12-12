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
                        continue;  // Start at row 4
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

            Dictionary<string, string> resultDictionary = resultList.ToDictionary(item => item[0], item => item[1]);

            return resultDictionary;
        }

    }
}
