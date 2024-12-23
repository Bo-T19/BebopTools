#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;


#endregion

namespace BebopTools
{
    internal class App : IExternalApplication
    {
        const string RIBBON_TAB = "Be-bop Tools";
        const string FIRST_RIBBON_PANEL = "GeneralTools";
        const string SECOND_RIBBON_PANEL = "ParameterTools";
        public Result OnStartup(UIControlledApplication a)
        {
            //Create the ribbon tab
            try
            {
                a.CreateRibbonTab(RIBBON_TAB);
            }
            catch (Exception)
            {
                //Tab already exists
            }


            //Create the panels
            RibbonPanel firstPanel = null;
            RibbonPanel secondPanel = null;
            List<RibbonPanel> panels = a.GetRibbonPanels(RIBBON_TAB);
            foreach (RibbonPanel ribbonPanel in panels)
            {
                if (ribbonPanel.Name == FIRST_RIBBON_PANEL)
                {
                    firstPanel = ribbonPanel;
                }
                else if (ribbonPanel.Name == SECOND_RIBBON_PANEL)
                {
                    secondPanel = ribbonPanel;
                }
            }

            //Couldn't find the panels

            if (firstPanel == null)
            {
                firstPanel = a.CreateRibbonPanel(RIBBON_TAB, FIRST_RIBBON_PANEL);
            }


            if (secondPanel == null)
            {
                secondPanel = a.CreateRibbonPanel(RIBBON_TAB, SECOND_RIBBON_PANEL);
            }


            //First Panel : General Tools
            
            //First button: Bridges Add-in

            //Get the image for the button
            Image bridges16 = Properties.Resources.Bridges16;
            ImageSource imageSourceBridges16 = GetImageSource(bridges16);


            Image bridges32 = Properties.Resources.Bridges32;
            ImageSource imageSourceBridges32 = GetImageSource(bridges32);

            //Create the button data
            PushButtonData bridgesPushButtonData = new PushButtonData("Bridges",
                                                     "Bridge \ngenerator",
                                                     Assembly.GetExecutingAssembly().Location,
                                                     "BebopTools.Command");
            //Add the button to the ribbon
            PushButton bridgesPushButton = firstPanel.AddItem(bridgesPushButtonData) as PushButton;
            bridgesPushButton.Image = imageSourceBridges16;
            bridgesPushButton.LargeImage = imageSourceBridges32;

          

            //Second button: Families Add-in

            //Get the image for the button
            Image families16 = Properties.Resources.Families16;
            ImageSource imageSourceFamilies16 = GetImageSource(families16);


            Image families32 = Properties.Resources.Families32;
            ImageSource imageSourceFamilies32 = GetImageSource(families32);

            //Create the button data
            PushButtonData familiesPushButtonData = new PushButtonData("Families",
                                                     "Family \nmanager",
                                                     Assembly.GetExecutingAssembly().Location,
                                                     "BebopTools.Command");
            //Add the button to the ribbon
            PushButton familiesPushButton = firstPanel.AddItem(familiesPushButtonData) as PushButton;
            familiesPushButton.Image = imageSourceFamilies16;
            familiesPushButton.LargeImage = imageSourceFamilies32;

            

            //Third button: Levels Add-in

            //Get the image for the button
            Image levels16 = Properties.Resources.Levels16;
            ImageSource imageSourceLevels16 = GetImageSource(levels16);


            Image levels32 = Properties.Resources.Levels32;
            ImageSource imageSourceLevels32 = GetImageSource(levels32);

            //Create the button data
            PushButtonData levelsPushButtonData = new PushButtonData("Levels",
                                                     "Level \nmanager",
                                                     Assembly.GetExecutingAssembly().Location,
                                                     "BebopTools.LevelsManager");
            //Add the button to the ribbon
            PushButton levelsPushButton = firstPanel.AddItem(levelsPushButtonData) as PushButton;
            levelsPushButton.Image = imageSourceLevels16;
            levelsPushButton.LargeImage = imageSourceLevels32;

            

            //Fourth button: Quantities Add-in

            //Get the image for the button
            Image quantities16 = Properties.Resources.Quantities16;
            ImageSource imageSourceQuantities16 = GetImageSource(quantities16);


            Image quantities32 = Properties.Resources.Quantities32;
            ImageSource imageSourceQuantities32 = GetImageSource(quantities32);

            //Create the button data
            PushButtonData quantitiesPushButtonData = new PushButtonData("Quantities",
                                                                 "Quantities \nmanager",
                                                                 Assembly.GetExecutingAssembly().Location,
                                                                 "BebopTools.Command");
            //Add the button to the ribbon
            PushButton quantitiesPushButton = firstPanel.AddItem(quantitiesPushButtonData) as PushButton;
            quantitiesPushButton.Image = imageSourceQuantities16;
            quantitiesPushButton.LargeImage = imageSourceQuantities32;


            //Second Panel: Parameter Tools

            //First button: Parameters Add-in

            //Get the image for the button
            Image parameters16 = Properties.Resources.Parameters16;
            ImageSource imageSourceParameters16 = GetImageSource(parameters16);


            Image parameters32 = Properties.Resources.Parameters32;
            ImageSource imageSourceParameters32 = GetImageSource(parameters32);

            //Create the button data
            PushButtonData parametersPushButtonData = new PushButtonData("Parameters",
                                                                 "Parameters \nmanager",
                                                                 Assembly.GetExecutingAssembly().Location,
                                                                 "BebopTools.FillParameters");
            //Add the button to the ribbon
            PushButton parametersPushButton = secondPanel.AddItem(parametersPushButtonData) as PushButton;
            parametersPushButton.Image = imageSourceParameters16;
            parametersPushButton.LargeImage = imageSourceParameters32;

            //Second button: Associate parameters Add-in

            //Get the image for the button
            Image associate16 = Properties.Resources.Associate16;
            ImageSource imageSourceAssociate16 = GetImageSource(associate16);


            Image associate32 = Properties.Resources.Associate32;
            ImageSource imageSourceAssociate32 = GetImageSource(associate32);

            //Create the button data
            PushButtonData associatePushButtonData = new PushButtonData("Associate",
                                                                 "Associate \nparameters",
                                                                 Assembly.GetExecutingAssembly().Location,
                                                                 "BebopTools.AssociateParameters");
            //Add the button to the ribbon
            PushButton associatePushButton = secondPanel.AddItem(associatePushButtonData) as PushButton;
            associatePushButton.Image = imageSourceAssociate16;
            associatePushButton.LargeImage = imageSourceAssociate32;

            //Third button: Download templates Add-in

            //Get the image for the button
            Image download16 = Properties.Resources.Download16;
            ImageSource imageSourceDownload16 = GetImageSource(download16);


            Image download32 = Properties.Resources.Download32;
            ImageSource imageSourceDownload32 = GetImageSource(download32);

            //Create the button data
            PushButtonData downloadPushButtonData = new PushButtonData("Download",
                                                                 "Download \ntemplates",
                                                                 Assembly.GetExecutingAssembly().Location,
                                                                 "BebopTools.DownloadTemplates");
            //Add the button to the ribbon
            PushButton downloadPushButton = secondPanel.AddItem(downloadPushButtonData) as PushButton;
            downloadPushButton.Image = imageSourceDownload16;
            downloadPushButton.LargeImage = imageSourceDownload32;


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }


        //This method is really important, it converts a PNG image into a BMP, which Revit can read and put as the 
        //ribbon icons
        private BitmapSource GetImageSource(Image image)
        {
            BitmapImage bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;

                bmp.EndInit();
            }

            return bmp;
        }
    }
}
