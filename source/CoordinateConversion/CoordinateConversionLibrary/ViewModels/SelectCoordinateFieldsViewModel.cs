using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateConversionLibrary.ViewModels
{
    public class SelectCoordinateFieldsViewModel : BaseViewModel
    {
        public SelectCoordinateFieldsViewModel()
        {
            AvailableFields = new ObservableCollection<string>();
            SelectedFields = new List<string>();
            InputTypes = new ObservableCollection<string>();
            InputTypes.Add("Single Field");
            InputTypes.Add("Lat Lon Fields");
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);
        }
        public ObservableCollection<string> AvailableFields { get; set; }
        public ObservableCollection<string> InputTypes { get; set; }
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
