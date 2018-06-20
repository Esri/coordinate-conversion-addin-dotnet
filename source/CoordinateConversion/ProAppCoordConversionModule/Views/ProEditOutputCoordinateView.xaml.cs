using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using System.Text.RegularExpressions;

namespace CoordinateConversionLibrary.Views
{
    /// <summary>
    /// Interaction logic for ProEditOutputCoordinateView.xaml
    /// </summary>
    public partial class ProEditOutputCoordinateView : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProEditOutputCoordinateView()
        {
            InitializeComponent();
        }

        public ProEditOutputCoordinateView(ObservableCollection<DefaultFormatModel> formats, List<string> names, OutputCoordinateModel outputCoordItem)
        {
            InitializeComponent();

            var vm = this.DataContext as EditOutputCoordinateViewModel;

            if (vm == null)
                return;

            vm.DefaultFormats = formats;
            vm.OutputCoordItem = outputCoordItem;
            vm.Names = names;

            var win = Window.GetWindow(this);

            if (win != null)
            {
                var temp = new System.Windows.Interop.WindowInteropHelper(win);

            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as EditOutputCoordinateViewModel;

            Regex alphanumericRegex = new Regex("^[a-zA-Z0-9]*$");
            Regex nonNumericStartRegex = new Regex("^(?![0-9])");
            Regex characterLimitRegex = new Regex("^[a-zA-Z0-9]{0,10}?$");

            if (vm == null)
                return;

            if (vm.Names.Contains(vm.OutputCoordItem.Name))
            {
                // no duplicates please
                e.Handled = false;
                MessageBox.Show(string.Format("The name '{0}' is already used.", vm.OutputCoordItem.Name));
                return;
            }
            else if (string.IsNullOrWhiteSpace(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                MessageBox.Show("Name is required.");
                return;
            }
            else if (!alphanumericRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                MessageBox.Show("The name should only contain alphabet and numbers.");
                return;
            }
            else if (!nonNumericStartRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                MessageBox.Show("The name should not start with a number.");
                return;
            }
            else if (!characterLimitRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                MessageBox.Show("The name must be 10 characters or less.");
                return;
            }

            DialogResult = true;
        }
    }
}
