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
    /// Interaction logic for SurfaceSelector.xaml
    /// </summary>
    public partial class ElementSurfaceSelector : Window
    {
        public List<string> Surfaces { get; set; }
        public ElementSurfaceSelector()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (Surfaces == null)
            {
                Surfaces = new List<string>();
            }


            if (Superiores.IsChecked == true)
            {
                Surfaces.Add("Superiores");
            }
            if (Laterales.IsChecked == true)
            {
                Surfaces.Add("Laterales");
            }
            if (Inferiores.IsChecked == true)
            {
                Surfaces.Add("Inferiores");
            }

            DialogResult = true;
        }
    }
}
