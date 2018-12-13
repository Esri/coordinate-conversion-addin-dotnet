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
    public class InputFormatHelper : ProTabBaseViewModel
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
                        if (ccc.GetType() == typeof(CoordinateMGRS))
                        {
                            result = new CoordinateMGRS(pointInformationMGRS.GZD, pointInformationMGRS.GS, pointInformationMGRS.Easting, pointInformationMGRS.Northing)
                                                        .ToString();
                        }
                        else
                        {
                            result = new CoordinateMGRS(pointInformationMGRS.GZD, pointInformationMGRS.GS, pointInformationMGRS.Easting, pointInformationMGRS.Northing)
                                                        .ToString("", new CoordinateMGRSFormatter());
                        }
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
                        var tmp = MapPointBuilder.FromGeoCoordinateString(mgrs.ToString(), sptlRef, GeoCoordinateType.MGRS, FromGeoCoordinateMode.Default);
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
