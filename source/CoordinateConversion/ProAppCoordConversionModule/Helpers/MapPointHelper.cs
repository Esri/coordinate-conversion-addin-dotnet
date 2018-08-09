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

using ArcGIS.Core.Geometry;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordConversionModule.Helpers
{
    public static class MapPointHelper
    {
        /// <summary>
        /// Helper method to get a string value of a MapPoint based on display configuration
        /// </summary>
        /// <param name="mp"></param>
        /// <returns></returns>
        public static string GetMapPointAsDisplayString(MapPoint mp)
        {
            if (mp == null)
                return "NA";

            var result = string.Format("{0:0.0#####} {1:0.0#####}", mp.Y, mp.X);

            // .ToGeoCoordinate function calls will fail if there is no Spatial Reference
            if (mp.SpatialReference == null)
                return result;

            ToGeoCoordinateParameter tgparam = null;

            try
            {
                switch (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType)
                {
                    case CoordinateTypes.DD:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DD);
                        tgparam.NumDigits = 6;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DDM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DDM);
                        tgparam.NumDigits = 4;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DMS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DMS);
                        tgparam.NumDigits = 2;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    //case CoordinateTypes.GARS:
                    //    tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.GARS);
                    //    result = mp.ToGeoCoordinateString(tgparam);
                    //    break;
                    case CoordinateTypes.MGRS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                        tgparam.Round = false;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.USNG:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.USNG);
                        tgparam.NumDigits = 5;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.UTM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.UTM);
                        tgparam.GeoCoordMode = ToGeoCoordinateMode.UtmNorthSouth;
                        result = mp.ToGeoCoordinateString(tgparam);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // do nothing
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
