using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.ViewModels;
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
            if (item == null)
                return;

            object obj = item.Content;

            ProCollectTabViewModel pCollVM = this.DataContext as ProCollectTabViewModel;
            pCollVM.SetListBoxItemAddInPoint.Execute(obj);
            e.Handled = true;
        }

        private void Import_Button_Click(object sender, RoutedEventArgs e)
        {
            this.listBoxCoordinates.UnselectAll();
        }

        private static bool IsMouseOverTarget(Visual target, Point point)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            return bounds.Contains(point);
        }

        private void listBoxCoordinates_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && Keyboard.Modifiers != ModifierKeys.Control)
            {
                for (int i = 0; i < listBoxCoordinates.Items.Count; i++)
                {
                    var lbi = listBoxCoordinates.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (lbi == null) continue;
                    if (IsMouseOverTarget(lbi, e.GetPosition((IInputElement)lbi)))
                    {
                        if (!lbi.IsSelected)
                        {
                            listBoxCoordinates.UnselectAll();
                            listBoxCoordinates.SelectedIndex = i;
                        }
                        break;
                    }
                }
            }
            else if (e.LeftButton != MouseButtonState.Pressed)
                e.Handled = true;
        }
    }
}
