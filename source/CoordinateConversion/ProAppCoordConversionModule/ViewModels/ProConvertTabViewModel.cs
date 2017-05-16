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
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProConvertTabViewModel : ProTabBaseViewModel
    {
        public ProConvertTabViewModel()
        {
            InputCCView = new InputCoordinateConversionView();
            InputCCView.DataContext = this;

            OutputCCView = new OutputCoordinateView();
            OutputCCView.DataContext = new OutputCoordinateViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new ProCollectTabViewModel();

            InputCoordinateHistoryList = new ObservableCollection<string>();

            IsActiveTab = true;
        }

        public InputCoordinateConversionView InputCCView { get; set; }
        public OutputCoordinateView OutputCCView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        #region overrides

        /// <summary>
        /// Override to include the update of input coordinate history
        /// </summary>
        /// <param name="obj"></param>
        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            var formattedInputCoordinate = proCoordGetter.GetInputDisplayString();

            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);

            // deactivate map point tool
            //IsToolActive = false;

            return true;
        }

        internal override async void OnFlashPointCommandAsync(object obj)
        {
            // Don't allow updating of the inputs or outputs while flashpoint is happening
            CoordinateMapTool.AllowUpdates = false;

            ProcessInput(InputCoordinate);
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

            if (obj == null)
                base.OnFlashPointCommandAsync(proCoordGetter.Point);
            else
                base.OnFlashPointCommandAsync(obj);
        }

        #endregion overrides
    }
}
