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

using System.Collections.ObjectModel;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class ConvertTabViewModel : ArcMapTabBaseViewModel
    {
        public ConvertTabViewModel()
        {
            InputCCView = new InputCoordinateConversionView();
            InputCCView.DataContext = this;

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

        public bool IsToolActive
        {
            get
            {
                if (ArcMap.Application.CurrentTool != null)
                    return ArcMap.Application.CurrentTool.Name == "Esri_ArcMapAddinCoordinateConversion_MapPointTool";
 
                return false;
            }
 
            set
            {
                if (value)
                {
                    CurrentTool = ArcMap.Application.CurrentTool;
                    OnActivatePointToolCommand(null);
                }                   
                else
                    ArcMap.Application.CurrentTool = CurrentTool;

                RaisePropertyChanged(() => IsToolActive);
                Mediator.NotifyColleagues("IsMapPointToolActive", value);
            }
        }
 
        /// <summary>
        /// Activates the map tool to get map points from mouse clicks/movement
        /// </summary>
        /// <param name="obj"></param>
        internal void OnActivateTool(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "Esri_ArcMapAddinCoordinateConversion_MapPointTool");
        }
 

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            string name = CoordinateType.DD.ToString();
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.AddNewOutputCoordinate, new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#####N X0.0#####E" });
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
        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            var formattedInputCoordinate = amCoordGetter.GetInputDisplayString();

            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);

            // deactivate map point tool
            IsToolActive = false;

            return true;
        }

        #endregion overrides
    }
}