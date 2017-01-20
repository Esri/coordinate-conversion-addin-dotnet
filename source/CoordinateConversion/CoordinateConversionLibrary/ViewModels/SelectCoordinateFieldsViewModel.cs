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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CoordinateConversionLibrary.Helpers;

namespace CoordinateConversionLibrary.ViewModels
{
    public class SelectCoordinateFieldsViewModel : BaseViewModel
    {
        public SelectCoordinateFieldsViewModel()
        {
            AvailableFields = new ObservableCollection<string>();
            SelectedFields = new List<string>();
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);
        }
        public ObservableCollection<string> AvailableFields { get; set; }
        public List<string> SelectedFields { get; set; }
        public string SelectedInputType { get; set; }
        private int selectedInputTypeIndex = 0;
        public int SelectedInputTypeIndex
        { 
            get { return selectedInputTypeIndex; } 
            set
            {
                selectedInputTypeIndex = value;
                RaisePropertyChanged(() => SelectedInputTypeIndex);
                RaisePropertyChanged(() => IsDialogComplete);
            }
        }
        private string selectedField1 = string.Empty;
        public string SelectedField1 
        {
            get { return selectedField1; }
            set
            {
                selectedField1 = value;
                RaisePropertyChanged(() => SelectedField1);
                RaisePropertyChanged(() => IsDialogComplete);
            }
        }
        private string selectedField2 = string.Empty;
        public string SelectedField2 
        {
            get { return selectedField2; }
            set
            {
                selectedField2 = value;
                RaisePropertyChanged(() => SelectedField2);
                RaisePropertyChanged(() => IsDialogComplete);
            }
        }
        
        public bool IsDialogComplete
        {
            get
            {
                if (SelectedInputTypeIndex == 0 && !string.IsNullOrWhiteSpace(SelectedField1))
                    return true;

                if (SelectedInputTypeIndex == 1 && !string.IsNullOrWhiteSpace(SelectedField1) && !string.IsNullOrWhiteSpace(SelectedField2))
                    return true;

                return false;
            }
        }

        public RelayCommand OKButtonPressedCommand { get; set; }

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
            if(SelectedInputTypeIndex == 0 || SelectedInputType == "Single Field")
            {
                if(!string.IsNullOrWhiteSpace(SelectedField1))
                    SelectedFields.Add(SelectedField1);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(SelectedField1))
                    SelectedFields.Add(SelectedField1);

                if (!string.IsNullOrWhiteSpace(SelectedField2))
                    SelectedFields.Add(SelectedField2);
            }

            // close dialog
            DialogResult = true;
        }

    }
}
