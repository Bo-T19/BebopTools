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
    /// Interaction logic for ElementSelectorForQuantities.xaml
    /// </summary>
    public partial class ElementSelectorForQuantities : Window
    {
        public string SelectionOption { get; set; }
        public ElementSelectorForQuantities()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            SelectionOption = SelectionOptions.Text;

            DialogResult = true;
        }
    }
}
