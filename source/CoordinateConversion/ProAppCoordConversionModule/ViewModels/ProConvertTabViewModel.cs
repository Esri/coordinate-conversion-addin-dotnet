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
using ProAppCoordConversionModule.Views;
using ProAppCoordConversionModule.ViewModels;
using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.Models;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;


namespace ProAppCoordConversionModule.ViewModels
{
    public class ProConvertTabViewModel : ProTabBaseViewModel
    {
        public ProConvertTabViewModel()
        {
            InputCCView = new InputCoordinateConversionView();
            InputCCView.DataContext = this;

            OutputCCView = new ProOutputCoordinateView();
            OutputCCView.DataContext = new ProOutputCoordinateViewModel();

            Module1.proOutputCoordVM = OutputCCView.DataContext as ProOutputCoordinateViewModel;

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new ProCollectTabViewModel();

            InputCoordinateHistoryList = new ObservableCollection<string>();
        }
  
        public InputCoordinateConversionView InputCCView { get; set; }
        public ProOutputCoordinateView OutputCCView { get; set; }
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
            // KG - Commented out so user can continously capture coordinates
            //IsToolActive = false;

            // KG - Added so output component will updated when user clicks on the map 
            //      not when mouse move event is fired.

            ViewModels.ProOutputCoordinateViewModel pOutCoordView = this.OutputCCView.DataContext as ViewModels.ProOutputCoordinateViewModel;
            pOutCoordView.RequestOutputCommand.Execute(null);
           
            return true;
        }

        internal override async void OnFlashPointCommandAsync(object obj)
        {
            if (MapView.Active == null)
            {
                System.Windows.Forms.MessageBox.Show(ProAppCoordConversionModule.Properties.Resources.LoadMapMsg);
                return;
            }

            // Don't allow updating of the inputs or outputs while flashpoint is happening
            CoordinateMapTool.AllowUpdates = false;

            ProcessInputValue(InputCoordinate);

            ViewModels.ProOutputCoordinateViewModel pOutCoordView = this.OutputCCView.DataContext as ViewModels.ProOutputCoordinateViewModel;
            pOutCoordView.RequestOutputCommand.Execute(null);

            await QueuedTask.Run(() =>
            {
                if (obj == null)
                {
                    base.OnFlashPointCommandAsync(proCoordGetter.Point);
                    AddCollectionPoint(proCoordGetter.Point as MapPoint);
                }
                else
                {
                    base.OnFlashPointCommandAsync(obj);
                    AddCollectionPoint(obj as MapPoint);
                }
            });
        }

        #endregion overrides

        private async void AddCollectionPoint(MapPoint point)
        {
            if (point != null)
            {
                var guid = await AddGraphicToMap(point, ColorFactory.Instance.RedRGB, true, 7);
                var addInPoint = new AddInPoint() { Point = point, GUID = guid };

                //Add point to the top of the list (using main thread)
                ArcGIS.Desktop.Framework.FrameworkApplication.Current.Dispatcher.Invoke(() =>
                    ProCollectTabViewModel.CoordinateAddInPoints.Add(addInPoint) );

            }
        }
    }
}
