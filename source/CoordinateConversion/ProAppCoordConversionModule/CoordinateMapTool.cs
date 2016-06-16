/******************************************************************************* 
  * Copyright 2015 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/

using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ProAppCoordConversionModule.UI;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.ViewModels;

namespace ProAppCoordConversionModule
{
    internal class CoordinateMapTool : MapTool
    {
        public CoordinateMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            UseSnapping = true;

            //Set the tools OverlayControlID to the DAML id of the embeddable control
            OverlayControlID = "ProAppCoordConversionModule_EmbeddableControl";
            Mediator.Register("UPDATE_FLASH", OnUpdateFlash);
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            ContextMenuID = "esri_mapping_popupToolContextMenu";
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mp = geometry as MapPoint;

            var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordConversionModule_CoordinateConversionDockpane") as CoordinateConversionDockpaneViewModel;
            if (vm != null)
            {
                vm.IsToolGenerated = true;
                vm.IsToolActive = false;
            }
            UpdateInputWithMapPoint(mp);

            return base.OnSketchCompleteAsync(geometry);
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordConversionModule_CoordinateConversionDockpane") as CoordinateConversionDockpaneViewModel;
            if (vm != null)
            {
                vm.IsHistoryUpdate = false;
            }
            UpdateInputWithMapPoint(e.ClientPoint);
        }

        private void OnUpdateFlash(object obj)
        {
            var mp = obj as MapPoint;

            if (mp != null)
                UpdateFlash(mp);
        }

        private void UpdateFlash(MapPoint mp)
        {
            System.Windows.Point? temp = new System.Windows.Point();

            var cp = QueuedTask.Run(() =>
            {
                if (MapView.Active != null)
                {
                    temp = MapView.Active.MapToClient(mp);
                }
                return temp;
            }).Result as System.Windows.Point?;

            UpdateFlash(cp);
        }

        private void UpdateFlash(System.Windows.Point? point)
        {
            var flashVM = OverlayEmbeddableControl as FlashEmbeddedControlViewModel;
            if (flashVM != null)
                flashVM.ClientPoint = point.Value;

            var temp = QueuedTask.Run(() =>
                {
                    if (flashVM != null && MapView.Active != null)
                    {
                        flashVM.ScreenPoint = MapView.Active.ClientToScreen(point.Value);
                        var p1 = MapView.Active.MapToScreen(MapPointBuilder.CreateMapPoint(MapView.Active.Extent.XMin, MapView.Active.Extent.YMin));
                        var p3 = MapView.Active.MapToScreen(MapPointBuilder.CreateMapPoint(MapView.Active.Extent.XMax, MapView.Active.Extent.YMax));
                        var width = (p3.X - p1.X) + 1;
                        var height = (p1.Y - p3.Y) + 1;
                        flashVM.MapWidth = width;
                        flashVM.MapHeight = height;
                    }
                    return true;
                }).Result;
        }

        /// <summary>
        /// Method to update the input coordinate text box
        /// </summary>
        /// <param name="mp">MapPoint</param>
        private void UpdateInputWithMapPoint(MapPoint mp)
        {
            if (mp != null)
            {
                if (CoordinateConversionViewModel.AddInConfig.DisplayCoordinateType != CoordinateConversionLibrary.CoordinateTypes.None)
                    mp = GeometryEngine.Project(mp, SpatialReferences.WGS84) as MapPoint;

                var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordConversionModule_CoordinateConversionDockpane") as CoordinateConversionDockpaneViewModel;
                if (vm != null)
                {
                    vm.InputCoordinate = string.Format("{0:0.0####} {1:0.0####}", mp.Y, mp.X);
                }
            }
        }

        /// <summary>
        /// Method to update the input coordinate text box
        /// </summary>
        /// <param name="e"></param>
        private void UpdateInputWithMapPoint(System.Windows.Point e)
        {
            var mp = QueuedTask.Run(() =>
            {
                MapPoint temp = null;

                if (MapView.Active != null)
                {
                    temp = MapView.Active.ClientToMap(e);
                    try
                    {
                        // for now we will always project to WGS84
                        if (CoordinateConversionViewModel.AddInConfig.DisplayCoordinateType != CoordinateConversionLibrary.CoordinateTypes.None)
                            temp = GeometryEngine.Project(temp, SpatialReferences.WGS84) as MapPoint;
                        
                        return temp;
                    }
                    catch { }
                }

                return temp;
            }).Result as MapPoint;

            if (mp != null)
            {
                var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordConversionModule_CoordinateConversionDockpane") as CoordinateConversionDockpaneViewModel;
                if (vm != null)
                {
                    vm.InputCoordinate = string.Format("{0:0.0####} {1:0.0####}", mp.Y, mp.X);
                }
            }
        }
    }
}
