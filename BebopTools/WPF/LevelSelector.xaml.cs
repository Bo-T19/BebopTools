using BebopTools.ParameterUtils;
using BebopTools.SelectionUtils;
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
    /// Interaction logic for LevelSelector.xaml
    /// </summary>
    public partial class LevelSelector : Window
    {

        private ElementsSelectionTools _elementsSelector;
        private ParametersManager _parametersManager;
        public List<string> LevelsNamesList { get; set; }
        public List<string> ParameterNamesList { get; set; }
        public List<string> SelectedLevels { get; set; }
        public string SelectedParameter { get; set; }
        public string SelectionOption { get; set; }


        public LevelSelector(ElementsSelectionTools elementsSelector, ParametersManager parametersManager)
        {
            InitializeComponent();
            _elementsSelector = elementsSelector;
            _parametersManager = parametersManager;

            LevelsNamesList = _elementsSelector.AllLevelsInProject();
            ParameterNamesList = _parametersManager.GetProjectParametersNames();

            DataContext = this;
            
        }


        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            SelectedLevels = LevelsList.SelectedItems
                                       .Cast<string>() 
                                       .ToList();

            SelectedParameter = ParametersList.Text;
            SelectionOption = LevelOptionsList.Text;

            DialogResult = true;
        }
    }
}
