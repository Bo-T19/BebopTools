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
using System.IO;

namespace BebopTools
{
    [Transaction(TransactionMode.Manual)]
    public class Detacher : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Application app = uiapp.Application;
                Document doc = uidoc.Document;

                //Sync with central options with its relinquish options and transaction options

                if (!doc.IsWorkshared)
                {
                    TaskDialog.Show("Error", "This is not a workshared model");
                    return Result.Succeeded;
                }

                TaskDialog.Show("Warning", "This will discard worksets!");

                SynchronizeWithCentralOptions synchronizeWithCentralOptions = new SynchronizeWithCentralOptions();
                RelinquishOptions relinquishOptions = new RelinquishOptions(true);
                TransactWithCentralOptions transactWithCentralOptions = new TransactWithCentralOptions();
                synchronizeWithCentralOptions.SetRelinquishOptions(relinquishOptions);

                doc.SynchronizeWithCentral(transactWithCentralOptions, synchronizeWithCentralOptions);



                //Path of the selected folder and name of the file
                string selectedPath = "";
                string[] fileNameParts = doc.PathName.Split('/');
                string fileName = fileNameParts[fileNameParts.Length - 1];

                //Open the folderSelector and get its values
                FolderSelector folderSelector = new FolderSelector();
                if (folderSelector.ShowDialog() == true)
                {
                    selectedPath = folderSelector.Path;
                }

                if (string.IsNullOrEmpty(selectedPath))
                {
                    TaskDialog.Show("Error", "You did not select a folder");
                    return Result.Failed;
                }
                string completeDocumentPath = Path.Combine(selectedPath, fileName);

                //Set the SaveAsOptions for the first Saved Document
                WorksharingSaveAsOptions worksharingOptions = new WorksharingSaveAsOptions
                {
                    SaveAsCentral = true,
                };

                SaveAsOptions worksharedSaveAsOptions = new SaveAsOptions
                {
                    OverwriteExistingFile = true,
                };

                worksharedSaveAsOptions.SetWorksharingOptions(worksharingOptions);


                doc.SaveAs(completeDocumentPath, worksharedSaveAsOptions);

                //Open the document and save it again, with its OpenOptions

                OpenOptions openOptions = new OpenOptions();
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets;

                ModelPath completeModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(completeDocumentPath);
                Document detachedDocument = app.OpenDocumentFile(completeModelPath, openOptions);


                SaveAsOptions saveAsOptions = new SaveAsOptions
                {
                    OverwriteExistingFile = true,
                };
                detachedDocument.SaveAs(completeDocumentPath, saveAsOptions);


                //Delete the 001 file that is created and the backup folder
                string file001Path = Path.Combine(selectedPath, Path.GetFileNameWithoutExtension(fileName) + ".0001.rvt");
                if (File.Exists(file001Path))
                {
                    File.Delete(file001Path);
                }
                string backupPath = Path.Combine(selectedPath, Path.GetFileNameWithoutExtension(fileName) + "_backup");
                if (Directory.Exists(backupPath))
                {
                    Directory.Delete(backupPath, true);
                }


                return Result.Succeeded;
            }
            catch (Exception ex) 
            {
                TaskDialog.Show("Error", $"Se produjo un error: {ex}");
                return Result.Failed;
            }
        }
    }
}
