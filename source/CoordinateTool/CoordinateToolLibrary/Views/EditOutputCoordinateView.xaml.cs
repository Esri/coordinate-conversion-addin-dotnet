using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.ViewModels;

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

        public EditOutputCoordinateView(ObservableCollection<DefaultFormatModel> formats, List<string> names, OutputCoordinateModel outputCoordItem)
        {
            InitializeComponent();

            var vm = this.DataContext as EditOutputCoordinateViewModel;

            if (vm == null)
                return;

            vm.DefaultFormats = formats;
            vm.OutputCoordItem = outputCoordItem;
            vm.Names = names;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as EditOutputCoordinateViewModel;

            if (vm == null)
                return;

            if(vm.Names.Contains(vm.OutputCoordItem.Name))
            {
                // no duplicates please
                e.Handled = false;
                MessageBox.Show(string.Format("The name '{0}' is already used.", vm.OutputCoordItem.Name));
                return;
            }

            if(string.IsNullOrWhiteSpace(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                MessageBox.Show("Name is required.");
                return;
            }

            DialogResult = true;
        }
    }
}
