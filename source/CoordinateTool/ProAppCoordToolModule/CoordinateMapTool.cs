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
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            var mp = QueuedTask.Run(() =>
            {
                MapPoint temp = null;

                if (MapView.Active != null)
                {
                    
                    temp = MapView.Active.ClientToMap(e.ClientPoint);
                }

                return temp;
            }).Result;

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
