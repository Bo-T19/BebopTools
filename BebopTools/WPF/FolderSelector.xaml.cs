using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace BebopTools.WPF
{
    /// <summary>
    /// Lógica de interacción para FolderSelector.xaml
    /// </summary>
    public partial class FolderSelector : Window
    {
        public string Path { get; set; }
        public FolderSelector()
        {
            InitializeComponent();
        }

        //OpenFolderDialog elemento for selecting the folder.
        private string OpenFolderDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a Folder",
                Filter = "Folders|\n", 
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection." 
            };

            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }


        private void Select_Click(object sender, RoutedEventArgs e)
        {
            PathTextBox.Text = OpenFolderDialog();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Path = PathTextBox.Text;


            DialogResult = true;
        }
    }
}
