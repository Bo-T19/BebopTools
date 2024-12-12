using BebopTools.ParameterUtils;
using Microsoft.Win32;
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

namespace BebopTools.WPF
{
    /// <summary>
    /// Interaction logic for FileSelectorForAssociateParams.xaml
    /// </summary>
    public partial class FileSelectorForAssociateParams : Window
    {
        private ParametersManager _parametersManager;
        public List<string> ParameterNamesList { get; set; }
        public string Path { get; set; }
        public string SelectedTargetParameter { get; set; }

        public string SelectedSourceParameter { get; set; }

        public FileSelectorForAssociateParams(ParametersManager parametersManager)
        {
            InitializeComponent();
            _parametersManager = parametersManager;

            ParameterNamesList = _parametersManager.GetProjectParametersNames();

            DataContext = this;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Excel files | *.xlsx";

            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                PathTextBox.Text = path;
            }

            else
            {
                throw new Exception("Did not pick a file");
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Path = PathTextBox.Text;
            SelectedTargetParameter = TargetParametersList.Text;
            SelectedSourceParameter = SourceParametersList.Text;

            DialogResult = true;
        }
    }
}
