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

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateSystemAddin.UI;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordConversionModule
{
    public class ProCoordinateGet : CoordinateConversionLibrary.Models.CoordinateGetBase
    {
        public ProCoordinateGet()
        { }

        public MapPoint Point { get; set; }

        #region Can Gets

        // use base CanGetDD

        public override bool CanGetDDM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if(base.CanGetDDM(srFactoryCode, out coord))
            {
                return true;
            }
            else
            {
                if(base.CanGetDD(srFactoryCode, out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if(CoordinateDD.TryParse(coord, out dd))
                    {
                        var ddm = new CoordinateDDM(dd);
                        coord = ddm.ToString("", new CoordinateDDMFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool CanGetDMS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (base.CanGetDMS(srFactoryCode, out coord))
            {
                return true;
            }
            else
            {
                if (base.CanGetDD(srFactoryCode, out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if (CoordinateDD.TryParse(coord, out dd))
                    {
                        var dms = new CoordinateDMS(dd);
                        coord = dms.ToString("", new CoordinateDMSFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool CanGetGARS(int srFacotryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.GARS);
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetMGRS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    // 5 numeric units in MGRS is 1m resolution
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUSNG(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.USNG);
                    tgparam.NumDigits = 5;
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUTM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.UTM);
                    tgparam.GeoCoordMode = ToGeoCoordinateMode.UtmNorthSouth;
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        private CoordSysDialog _dlg = null;
        private static bool _isOpen = false;

        public override bool SelectSpatialReference()
        {
            if (_isOpen)
                return false;

            _isOpen = true;
            _dlg = new CoordSysDialog();
            _dlg.Closing += bld_Closing;
            _dlg.Show();

            return false;
        }

        private void bld_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_dlg.SpatialReference != null)
            {
                System.Windows.MessageBox.Show(string.Format("You picked {0}", _dlg.SpatialReference.Name), "Pick Coordinate System");
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SpatialReferenceSelected, string.Format("{0}::{1}", _dlg.SpatialReference.Wkid, _dlg.SpatialReference.Name));
            }
            _dlg = null;
            _isOpen = false;
        }

        public override void Project(int factoryCode)
        {
            var temp = QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.SpatialReference spatialReference = SpatialReferenceBuilder.CreateSpatialReference(factoryCode);

                Point = (MapPoint)GeometryEngine.Project(Point, spatialReference);

                return true;
            }).Result;
        }
        #endregion
    }
}
