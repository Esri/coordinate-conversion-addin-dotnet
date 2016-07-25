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
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class ConvertTabViewModel : TabBaseViewModel
    {
        public ConvertTabViewModel()
        {
            InputCCView = new InputCoordinateConversionView();
            //TODO need to decide what to do with this
            InputCCView.DataContext = this;// new CoordinateConversionViewModel();

            OutputCCView = new OutputCoordinateView();
            OutputCCView.DataContext = new OutputCoordinateViewModel();

            InputCoordinateHistoryList = new ObservableCollection<string>();

            // commands
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            CopyAllCommand = new RelayCommand(OnCopyAllCommand);
        }

        public InputCoordinateConversionView InputCCView { get; set; }
        public OutputCoordinateView OutputCCView { get; set; }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        public RelayCommand AddNewOCCommand { get; set; }
        public RelayCommand CopyAllCommand { get; set; }

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            string name = CoordinateType.DD.ToString();
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.AddNewOutputCoordinate, new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#N X0.0#E" });
        }
        
        private void OnCopyAllCommand(object obj)
        {
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.CopyAllCoordinateOutputs, InputCoordinate);
        }

        #region overrides

        /// <summary>
        /// Override to include the update of input coordinate history
        /// </summary>
        /// <param name="obj"></param>
        internal override void OnNewMapPoint(object obj)
        {
            base.OnNewMapPoint(obj);

            if (!IsActiveTab)
                return;
            
            var formattedInputCoordinate = amCoordGetter.GetInputDisplayString();
            
            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);
        }

        #endregion overrides
    }
}
