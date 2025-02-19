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

namespace BebopTools.WPF.DimensionerWPFs
{
    /// <summary>
    /// Lógica de interacción para WallDimensionerOptions.xaml
    /// </summary>
    public partial class WallDimensionerOptions : Window
    {
        public bool DimensionAllWalls { get; set; }
        public bool DimensionWidths { get; set; }

        public WallDimensionerOptions()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DimensionWidths = Widths.IsChecked ?? false;
            DimensionAllWalls = AllWalls.IsChecked ?? false;
            DialogResult = true;
        }
    }
}
