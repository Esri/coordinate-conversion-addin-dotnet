using CoordinateConversionLibrary.Models;
using System.Windows;

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

        private void OnDontShowAgainClick(object sender, RoutedEventArgs e)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = !cbDontShowAgain.IsChecked.Value;
        }
    }
}
