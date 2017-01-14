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
using CoordinateSystemAddin.UI;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using CoordinateConversionLibrary;
using System;

namespace ProAppCoordConversionModule
{
    public class ProCoordinateGet : CoordinateConversionLibrary.Models.CoordinateGetBase
    {
        public ProCoordinateGet()
        { }

        public MapPoint Point { get; set; }

        #region Can Gets

        // use base CanGetDD

        public override bool CanGetDD(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DD);
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDDM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DDM);
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDMS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DMS);
                    coord = Point.ToGeoCoordinateString(tgparam);
                    return true;
                }
                catch { }
            }
            return false;
        }

        /*public override bool CanGetGARS(int srFacotryCode, out string coord)
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
        }*/

        public override bool CanGetMGRS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    // 5 numeric units in MGRS is 1m resolution
                    var tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                    tgparam.Round = false;
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
                    tgparam.GeoCoordMode = ToGeoCoordinateMode.Default;
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

        public string GetInputDisplayString()
        {
            if (Point == null)
                return "NA";

            var result = string.Format("{0:0.0#####} {1:0.0#####}", Point.Y, Point.X);

            if (Point.SpatialReference == null)
                return result;

            ToGeoCoordinateParameter tgparam = null;

            try
            {
                switch (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType)
                {
                    case CoordinateTypes.DD:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DD);
                        tgparam.NumDigits = 6;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DDM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DDM);
                        tgparam.NumDigits = 4;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DMS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DMS);
                        tgparam.NumDigits = 2;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    //case CoordinateTypes.GARS:
                        //tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.GARS);
                        //result = Point.ToGeoCoordinateString(tgparam);
                        //break;
                    case CoordinateTypes.MGRS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                        tgparam.Round = false;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.USNG:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.USNG);
                        tgparam.NumDigits = 5;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.UTM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.UTM);
                        tgparam.GeoCoordMode = ToGeoCoordinateMode.UtmNorthSouth;
                        result = Point.ToGeoCoordinateString(tgparam);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                // do nothing
            }
            return result;
        }

    }
}
