using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BebopTools.ParameterUtils;
using BebopTools.DownloadAndUploadUtils;
using BebopTools.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using BebopTools.SelectionUtils;

namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    public class Detacher : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Path of the selected folder
            string selectedPath = "";

            //Open the folderSelector and get its values
            FolderSelector folderSelector = new FolderSelector();
            if (folderSelector.ShowDialog() == true)
            {
                selectedPath = folderSelector.Path;
            }

            //Sync with central options with its relinquish options and transaction options

            SynchronizeWithCentralOptions synchronizeWithCentralOptions = new SynchronizeWithCentralOptions();
            RelinquishOptions relinquishOptions = new RelinquishOptions(true);
            TransactWithCentralOptions transactWithCentralOptions = new TransactWithCentralOptions();
            synchronizeWithCentralOptions.SetRelinquishOptions(relinquishOptions);

            doc.SynchronizeWithCentral(transactWithCentralOptions, synchronizeWithCentralOptions);


            return Result.Succeeded;
        }
    }
}
