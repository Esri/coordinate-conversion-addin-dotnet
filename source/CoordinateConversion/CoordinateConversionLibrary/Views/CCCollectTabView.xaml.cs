using CoordinateConversionLibrary.Helpers;
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
    /// Interaction logic for BatchCoordinateView.xaml
    /// </summary>
    public partial class CCCollectTabView : UserControl
    {
        public CCCollectTabView()
        {
            InitializeComponent();
        }

        private void listBoxItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            object obj = item.Content;
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, obj);
            e.Handled = true;
        }
        private void listBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, null);
        }
    }
}
