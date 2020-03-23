using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using ProAppCoordConversionModule.Models;
using ProAppCoordConversionModule.ViewModels;
using System.Text.RegularExpressions;

namespace ProAppCoordConversionModule.Views
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

            if (vm == null)
                return;

            Regex alphanumericRegex = new Regex("^[a-zA-Z0-9]*$");
            Regex nonNumericStartRegex = new Regex("^(?![0-9])");
            Regex characterLimitRegex = new Regex("^[a-zA-Z0-9]{0,10}?$");

            if (vm.Names.Contains(vm.OutputCoordItem.Name))
            {
                // no duplicates please
                e.Handled = false;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(string.Format(Properties.Resources.MsgThe + " '{0}' " + Properties.Resources.Msgis, vm.OutputCoordItem.Name));
                return;
            }
            else if (string.IsNullOrWhiteSpace(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Properties.Resources.MsgErrorName);
                return;
            }
            else if (!alphanumericRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Properties.Resources.MsgOthers);
                return;
            }
            else if (!nonNumericStartRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Properties.Resources.MsgNumber);
                return;
            }
            else if (!characterLimitRegex.IsMatch(vm.OutputCoordItem.Name))
            {
                e.Handled = false;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Properties.Resources.MsgLess);
                return;
            }

            DialogResult = true;
        }
    }
}
