using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BebopTools.DownloadUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    internal class DownloadTemplates : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Desktop Path
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                // Call the method
                FileDownloader.DownloadExcelFiles(desktopPath);

                // Success!
                TaskDialog.Show("Descarga Completada", "Templates successfully downloaded to your desktop");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                
                TaskDialog.Show("Error", $"It was not possible to download the files: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
