/******************************************************************************* 
  * Copyright 2016 Esri 
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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using CoordinateToolLibrary.Models;

namespace ProAppCoordToolModule
{
    internal class ContextCopyBase : Button
    {
        internal ContextCopyBase()
        {
        }

        internal CoordinateType cType = CoordinateType.Unknown;

        protected async override void OnClick()
        {
            if (MapView.Active == null || MapView.Active.Map == null)
                return;

            // get screen point
            var temp = (System.Windows.Point)ArcGIS.Desktop.Framework.FrameworkApplication.ContextMenuDataContext;
            MapPoint mp = null;

            if (temp != null)
            {
                mp = QueuedTask.Run(() =>
                {
                    MapPoint tmp = null;

                    tmp = MapView.Active.ScreenToMap(temp);
                    return tmp;
                }).Result as MapPoint;

            }

            if (mp == null)
                return;

            string coord = String.Empty;
            ToGeoCoordinateParameter tgparam;

            try
            {
                switch (cType)
                {
                    case CoordinateType.DD:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DD);
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.DDM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DDM);
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.DMS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DMS);
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.GARS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.GARS);
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.MGRS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.USNG:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.USNG);
                        tgparam.NumDigits = 5;
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateType.UTM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.UTM);
                        tgparam.GeoCoordMode = ToGeoCoordinateMode.UtmNorthSouth;
                        coord = mp.ToGeoCoordinateString(tgparam);
                        break;
                    default:
                        break;
                }

                var vm = FrameworkApplication.DockPaneManager.Find("ProAppCoordToolModule_CoordinateToolDockpane") as CoordinateToolDockpaneViewModel;
                if (vm != null)
                {
                    coord = vm.GetFormattedCoordinate(coord, cType);
                }

                System.Windows.Clipboard.SetText(coord);
            }
            catch {}

        }
    }

    internal class ContextCopyDD : ContextCopyBase
    {
        ContextCopyDD()
        {
            cType = CoordinateType.DD;
        }
    }

    internal class ContextCopyDDM : ContextCopyBase
    {
        ContextCopyDDM()
        {
            cType = CoordinateType.DDM;
        }
    }

    internal class ContextCopyDMS : ContextCopyBase
    {
        ContextCopyDMS()
        {
            cType = CoordinateType.DMS;
        }
    }
    internal class ContextCopyGARS : ContextCopyBase
    {
        ContextCopyGARS()
        {
            cType = CoordinateType.GARS;
        }
    }
    internal class ContextCopyMGRS : ContextCopyBase
    {
        ContextCopyMGRS()
        {
            cType = CoordinateType.MGRS;
        }
    }
    internal class ContextCopyUSNG : ContextCopyBase
    {
        ContextCopyUSNG()
        {
            cType = CoordinateType.USNG;
        }
    }
    internal class ContextCopyUTM : ContextCopyBase
    {
        ContextCopyUTM()
        {
            cType = CoordinateType.UTM;
        }
    }
}
