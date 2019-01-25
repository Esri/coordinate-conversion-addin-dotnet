using CoordinateConversionLibrary.Models;
using System.Windows;

namespace CoordinateConversionLibrary.Views
{
    /// <summary>
    /// Interaction logic for AmbiguousCoordsView.xaml
    /// </summary>
    public partial class AmbiguousCoordsView : Window
    {
        public bool CheckedLatLon
        {
            get { return _checkedLatLon; }
            set
            {
                _checkedLatLon = value;
                _checkedLonLat = !_checkedLatLon;
            }
        }
        bool _checkedLatLon;

        public bool CheckedLonLat
        {
            get { return _checkedLonLat; }
            set
            {
                _checkedLonLat = value;
                _checkedLatLon = !_checkedLonLat;
            }
        }
        bool _checkedLonLat;

        public AmbiguousCoordsView()
        {
            InitializeComponent();

            this.DataContext = this;

            CheckedLatLon = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            CoordinateConversionLibraryConfig.AddInConfig.isLatLong = CheckedLatLon;
        }

        private void OnDontShowAgainClick(object sender, RoutedEventArgs e)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = !cbDontShowAgain.IsChecked.Value;
        }
    }
}
