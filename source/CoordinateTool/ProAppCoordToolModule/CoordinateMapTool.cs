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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ProAppCoordToolModule.UI;
using CoordinateToolLibrary.Helpers;

namespace ProAppCoordToolModule
{
    internal class CoordinateMapTool : MapTool
    {
        public CoordinateMapTool()
        {
            //Set the tools OverlayControlID to the DAML id of the embeddable control
            OverlayControlID = "ProAppCoordToolModule_EmbeddableControl";
            Mediator.Register("UPDATE_FLASH", OnUpdateFlash);
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            ContextMenuID = "esri_mapping_popupToolContextMenu";
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left)
                return;

            var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordToolModule_CoordinateToolDockpane") as CoordinateToolDockpaneViewModel;
            if (vm != null)
            {
                vm.IsHistoryUpdate = true;
                vm.IsToolActive = false;
            }
            UpdateInputWithMapPoint(e.ClientPoint);
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
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
                        flashVM.ScreenPoint = MapView.Active.ClientToScreen(point.Value);
                    return true;
                }).Result;
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
                        var result = GeometryEngine.Project(temp, SpatialReferences.WGS84);
                        return result;
                    }
                    catch { }
                }

                return temp;
            }).Result as MapPoint;

            if (mp != null)
            {
                var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordToolModule_CoordinateToolDockpane") as CoordinateToolDockpaneViewModel;
                if (vm != null)
                {
                    vm.InputCoordinate = string.Format("{0:0.0####} {1:0.0####}", mp.Y, mp.X);
                }
            }
        }
    }
}
