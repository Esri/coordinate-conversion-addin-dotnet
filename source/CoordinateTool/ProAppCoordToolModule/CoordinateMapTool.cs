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

namespace ProAppCoordToolModule
{
    internal class CoordinateMapTool : MapTool
    {
        protected override Task OnToolActivateAsync(bool active)
        {
            ContextMenuID = "esri_mapping_popupToolContextMenu";
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left)
                return;

            UpdateInputWithMapPoint(e.ClientPoint);

            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
           UpdateInputWithMapPoint(e.ClientPoint);
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
