using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.Models;
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

namespace ProAppCoordConversionModule.Views
{
    /// <summary>
    /// Interaction logic for InputCoordinateConversionView.xaml
    /// </summary>
    public partial class InputCoordinateConversionView : UserControl
    {
        public InputCoordinateConversionView()
        {
            InitializeComponent();
        }

        private void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
            ViewModels.ProConvertTabViewModel pConvertTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;
            ViewModels.ProOutputCoordinateViewModel pOutCoordVM = pConvertTabVM.OutputCCView.DataContext as ViewModels.ProOutputCoordinateViewModel;
            pOutCoordVM.ClearOutputCoordinates.Execute(null);
        }
    }
}
