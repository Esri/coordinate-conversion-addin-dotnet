using CoordinateToolLibrary.ViewModels;
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

namespace CoordinateToolLibrary.Views
{
    /// <summary>
    /// Interaction logic for EditOutputCoordinateView.xaml
    /// </summary>
    public partial class EditOutputCoordinateView : Window
    {
        public EditOutputCoordinateView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as EditOutputCoordinateViewModel;

            if (vm == null)
                return;

            vm.UpdateSample();
        }

        private void FormatListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as EditOutputCoordinateViewModel;

            if (vm == null)
                return;

            vm.UpdateFormat();
        }
    }
}
