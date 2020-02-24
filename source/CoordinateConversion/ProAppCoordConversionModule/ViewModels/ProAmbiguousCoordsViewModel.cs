using ProAppCoordConversionModule.Common.Enums;
using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.Models;
using ProAppCoordConversionModule.ViewModels;

namespace ProAppCoordConversionModule.ViewModels
{
    public partial class ProAmbiguousCoordsViewModel : NotificationObject
    {
        public ProAmbiguousCoordsViewModel()
        {
            SelectedCoordinateType = CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType;
            DisplayAmbiguousCoordsDlg = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
            CheckedLatLon = true;
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);
            DontShowAgainCommand = new RelayCommand(OnDontShowAgainCommand);
        }

        #region Properties
        public CoordinateTypes SelectedCoordinateType { get; set; }
        public bool DisplayAmbiguousCoordsDlg { get; set; }
        public RelayCommand OKButtonPressedCommand { get; set; }
        public RelayCommand DontShowAgainCommand { get; set; }
        public bool IsDontShowAgainChecked { get; set; }
        private bool? dialogResult = null;
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                RaisePropertyChanged(() => DialogResult);
            }
        }
        private bool _checkedLatLon;
        private bool _checkedLonLat;

        public bool CheckedLatLon
        {
            get { return _checkedLatLon; }
            set
            {
                _checkedLatLon = value;
                _checkedLonLat = !_checkedLatLon;
                RaisePropertyChanged(() => CheckedLatLon);
            }
        }

        public bool CheckedLonLat
        {
            get { return _checkedLonLat; }
            set
            {
                _checkedLonLat = value;
                _checkedLatLon = !_checkedLonLat;
                RaisePropertyChanged(() => CheckedLonLat);
            }
        }

        #endregion

        #region Commands
        private void OnOkButtonPressedCommand(object obj)
        {
            DialogResult = true;
        }

        private void OnDontShowAgainCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = !IsDontShowAgainChecked;
        }
        #endregion
    }
}
