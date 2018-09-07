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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Models;
using ProAppCoordConversionModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            if (CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
            {
                ProcessData processData = new ProcessData();
                result = processData.ProcessInput(result);
            }
            return result;
        }


    }

    public class ProcessData : ProTabBaseViewModel
    {
        public override string ProcessInput(string input)
        {
            if (input == "NA") return string.Empty;

            string result = string.Empty;
            //MapPoint point;
            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            //var coordType = GetCoordinateType(input, out point);
            // must force non async here to avoid returning to base class early
            var ccc = QueuedTask.Run(() =>
            {
                return GetCoordinateType(input);
            }).Result;


            if (ccc.Type == CoordinateType.Unknown)
            {
                HasInputError = true;
                proCoordGetter.Point = null;
                foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    output.OutputCoordinate = "";
                    output.Props.Clear();
                }
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.InvalidCoordMsg,
                    CoordinateConversionLibrary.Properties.Resources.InvalidCoordCap);
            }
            else
            {
                proCoordGetter.Point = ccc.Point;
                var inputCategorySelection = Enum.GetValues(typeof(CoordinateTypes)).Cast<CoordinateTypes>().Where(x => x.ToString() == ccc.Type.ToString()).FirstOrDefault();
                //switch (CoordinateBase.InputCategorySelection)
                switch (inputCategorySelection)
                {
                    case CoordinateTypes.DD:
                        if (ccc.GetType() == typeof(CoordinateDD))
                        {
                            var pointInformationDD = (CoordinateDD)ccc.PointInformation;
                            result = new CoordinateDD(pointInformationDD.Lat, pointInformationDD.Lon).ToString("", new CoordinateDDFormatter());
                        }
                        else
                        {
                            result = new CoordinateDD(ccc.Point.Y, ccc.Point.X).ToString("", new CoordinateDDFormatter());
                        }
                        break;
                    case CoordinateTypes.DDM:
                        if (ccc.GetType() == typeof(CoordinateDDM))
                        {
                            var pointInformationDDM = (CoordinateDDM)ccc.PointInformation;
                            result = new CoordinateDDM(pointInformationDDM.LatDegrees, pointInformationDDM.LatMinutes, pointInformationDDM.LonDegrees, pointInformationDDM.LonMinutes)
                                .ToString("", new CoordinateDDMFormatter());
                        }
                        else
                        {
                            result = new CoordinateDDM(new CoordinateDD(ccc.Point.Y, ccc.Point.X))
                                .ToString("", new CoordinateDDMFormatter());
                        }
                        break;
                    case CoordinateTypes.DMS:
                        if (ccc.GetType() == typeof(CoordinateDMS))
                        {
                            var pointInformationDMS = (CoordinateDMS)ccc.PointInformation;
                            result = new CoordinateDMS(pointInformationDMS.LatDegrees, pointInformationDMS.LatMinutes, pointInformationDMS.LatSeconds,
                                                        pointInformationDMS.LonDegrees, pointInformationDMS.LonMinutes, pointInformationDMS.LonSeconds)
                                                        .ToString("", new CoordinateDMSFormatter());
                        }
                        else
                        {
                            result = new CoordinateDMS(new CoordinateDD(ccc.Point.Y, ccc.Point.X))
                                .ToString("", new CoordinateDMSFormatter());
                        }
                        break;
                    case CoordinateTypes.MGRS:
                        var pointInformationMGRS = (CoordinateMGRS)ccc.PointInformation;
                        result = new CoordinateMGRS(pointInformationMGRS.GZD, pointInformationMGRS.GS, pointInformationMGRS.Easting, pointInformationMGRS.Northing)
                                                    .ToString();
                        break;
                    case CoordinateTypes.USNG:
                        var pointInformationUSNG = (CoordinateUSNG)ccc.PointInformation;
                        result = new CoordinateUSNG(pointInformationUSNG.GZD, pointInformationUSNG.GS, pointInformationUSNG.Easting, pointInformationUSNG.Northing)
                                                    .ToString();
                        break;
                    case CoordinateTypes.UTM:
                        var pointInformationUTM = (CoordinateUTM)ccc.PointInformation;
                        result = new CoordinateUTM(pointInformationUTM.Zone, pointInformationUTM.Band, pointInformationUTM.Easting, pointInformationUTM.Northing)
                                                    .ToString();
                        break;
                    case CoordinateTypes.None:
                        break;
                    case CoordinateTypes.Custom:
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private async Task<CCCoordinate> GetCoordinateType(string input)
        {
            MapPoint point = null;

            // DD
            CoordinateDD dd;
            if (CoordinateDD.TryParse(input, out dd, true))
            {
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DD, Point = point, PointInformation = dd };
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm, true))
            {
                dd = new CoordinateDD(ddm);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DDM, Point = point, PointInformation = ddm };
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms, true))
            {
                dd = new CoordinateDD(dms);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DMS, Point = point, PointInformation = dms };
            }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(gars.ToString("", new CoordinateGARSFormatter()), sptlRef, GeoCoordinateType.GARS, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.GARS, Point = point, PointInformation = gars };
                }
                catch { }
            }

            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(mgrs.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.MGRS, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.MGRS, Point = point, PointInformation = mgrs };
                }
                catch { }
            }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(usng.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.USNG, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.USNG, Point = point, PointInformation = usng };
                }
                catch { }
            }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(utm.ToString("", new CoordinateUTMFormatter()), sptlRef, GeoCoordinateType.UTM, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.UTM, Point = point, PointInformation = utm };
                }
                catch { }
            }

            /*
             * Updated RegEx to capture invalid coordinates like 00, 45, or 456987. 
             */
            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]{1,}(?<longitude>\-?\d+[.,]?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
                    point = await QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
                    });//.Result;
                    return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
                }
                catch (Exception)
                {
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                }
            }

            return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
        }
    }
}
