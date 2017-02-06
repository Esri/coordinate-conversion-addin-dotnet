// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;

namespace CoordinateConversionLibrary.ViewModels
{
    public class EditPropertiesViewModel : BaseViewModel
    {
        public EditPropertiesViewModel()
        {
            SelectedCoordinateType = CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType;
            DisplayAmbiguousCoordsDlg = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);

        }

        public RelayCommand OKButtonPressedCommand { get; set; }

        public CoordinateTypes SelectedCoordinateType { get; set; }

        public bool DisplayAmbiguousCoordsDlg { get; set; }

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

        /// <summary>
        /// Handler for when someone closes the dialog with the OK button
        /// </summary>
        /// <param name="obj"></param>
        private void OnOkButtonPressedCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType = SelectedCoordinateType;

            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = DisplayAmbiguousCoordsDlg;

            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();

            // close dialog
            DialogResult = true;
        }
    }
}
