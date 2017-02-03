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

namespace CoordinateConversionLibrary.Views
{
    /// <summary>
    /// Interaction logic for AmbiguousCoordsView.xaml
    /// </summary>
    public partial class AmbiguousCoordsView : Window
    {
        public AmbiguousCoordsView()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
