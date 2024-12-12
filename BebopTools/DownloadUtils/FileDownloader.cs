using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace BebopTools.DownloadUtils
{
    internal class FileDownloader
    {
        // This class has the method to download the Excel templates to a path given by the user
        public static void DownloadExcelFiles(string destinationPath)
        {
            try
            {
                // Ensure the destination folder exists
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                // Define the fully qualified names of the embedded resources
                string resourceFile1 = "BebopTools.Resources.AssociateParameters.xlsx";
                string resourceFile2 = "BebopTools.Resources.FillParameters.xlsx";

                // Extract and copy the Excel templates from embedded resources to the destination path
                ExtractFile(resourceFile1, Path.Combine(destinationPath, "AssociateParameters.xlsx"));
                ExtractFile(resourceFile2, Path.Combine(destinationPath, "FillParameters.xlsx"));
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the process
                throw new Exception($"It was not possible to download the files: {ex.Message}");
            }
        }

        // Method to extract a file from embedded resources and save it to a specified path
        private static void ExtractFile(string resourceName, string destinationPath)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new FileNotFoundException($"Embedded resource {resourceName} not found.");

                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }
    }
}
