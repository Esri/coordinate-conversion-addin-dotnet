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
using System.Windows.Forms;

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

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new CollectTabViewModel();

            InputCoordinateHistoryList = new ObservableCollection<string>();
        }

        public InputCoordinateConversionView InputCCView { get; set; }
        public OutputCoordinateView OutputCCView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        public bool IsToolActive
        {
            get
            {
                if ((ArcMap.Application != null) && (ArcMap.Application.CurrentTool != null))
                    return ArcMap.Application.CurrentTool.Name.ToLower() == MapPointToolName.ToLower();

                return false;
            }

            set
            {
                if (value)
                {
                    if (ArcMap.Application != null)
                        CurrentTool = ArcMap.Application.CurrentTool;

                    OnActivateTool(null);
                }
                else
                {
                    ArcMap.Application.CurrentTool = CurrentTool;
                }

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
            if (ArcMap.LayerCount > 0)
            {
                SetToolActiveInToolBar(ArcMap.Application, MapPointToolName);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.AddLayerMsg,
                    CoordinateConversionLibrary.Properties.Resources.AddLayerCap);
            }            
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
            // KG - Commented out so user can continously capture coordinates
            //IsToolActive = false;

            // KG - Added so output component will updated when user clicks on the map 
            //      not when mouse move event is fired.
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

            return true;
        }

        #endregion overrides
    }
}