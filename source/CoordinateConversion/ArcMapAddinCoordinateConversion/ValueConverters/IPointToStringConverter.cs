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

using System;
using System.Windows.Data;
using ESRI.ArcGIS.Geometry;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ArcMapAddinCoordinateConversion.Helpers;
using System.Linq;

namespace ArcMapAddinCoordinateConversion.ValueConverters
{
    [ValueConversion(typeof(IPoint), typeof(String))]
    public class IPointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "NA";

            var point = value as IPoint;
            if (point == null)
                return "NA";

            var result = string.Format("{0:0.0#####} {1:0.0#####}", point.Y, point.X);

            if (point.SpatialReference == null)
                return result;

            var cn = point as IConversionNotation;
            if (cn != null)
            {
                switch (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType)
                {
                    case CoordinateTypes.DD:
                        result = cn.GetDDFromCoords(6);
                        break;
                    case CoordinateTypes.DDM:
                        result = cn.GetDDMFromCoords(4);
                        break;
                    case CoordinateTypes.DMS:
                        result = cn.GetDMSFromCoords(2);
                        break;
                    //case CoordinateTypes.GARS:
                    //    result = cn.GetGARSFromCoords();
                    //    break;
                    case CoordinateTypes.MGRS:
                        result = cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
                        break;
                    case CoordinateTypes.USNG:
                        result = cn.GetUSNGFromCoords(5, true, true);
                        break;
                    case CoordinateTypes.UTM:
                        result = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces | esriUTMConversionOptionsEnum.esriUTMUseNS);
                        break;
                    default:
                        break;
                }
            }
            if (CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
            {
                ProcessData processData = new ProcessData();
                result = processData.ProcessInput(result);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProcessData
    {
        public string ProcessInput(string inputData)
        {
            CoordinateBase PointInformation;
            ESRI.ArcGIS.Geometry.IPoint point;
            var result = "";

            var ctype = GetCoordinateType(inputData, out point, out PointInformation);
            var inputCategorySelection = Enum.GetValues(typeof(CoordinateTypes)).Cast<CoordinateTypes>().Where(x => x.ToString() == ctype.ToString()).FirstOrDefault();
            switch (inputCategorySelection)
            {
                case CoordinateTypes.DD:
                    if (ctype.ToString() == CoordinateTypes.DD.ToString())
                    {
                        var pointInformationDD = (CoordinateDD)PointInformation;
                        result = new CoordinateDD(pointInformationDD.Lat, pointInformationDD.Lon).ToString("", new CoordinateDDFormatter());
                    }
                    else
                    {
                        result = new CoordinateDD(point.Y, point.X).ToString("", new CoordinateDDFormatter());
                    }
                    break;
                case CoordinateTypes.DDM:
                    if (ctype.ToString() == CoordinateTypes.DDM.ToString())
                    {
                        var pointInformationDDM = (CoordinateDDM)PointInformation;
                        result = new CoordinateDDM(pointInformationDDM.LatDegrees, pointInformationDDM.LatMinutes, pointInformationDDM.LonDegrees, pointInformationDDM.LonMinutes)
                            .ToString("", new CoordinateDDMFormatter());
                    }
                    else
                    {
                        result = new CoordinateDDM(new CoordinateDD(point.Y, point.X))
                            .ToString("", new CoordinateDDMFormatter());
                    }
                    break;
                case CoordinateTypes.DMS:
                    if (ctype.ToString() == CoordinateTypes.DMS.ToString())
                    {
                        var pointInformationDMS = (CoordinateDMS)PointInformation;
                        result = new CoordinateDMS(pointInformationDMS.LatDegrees, pointInformationDMS.LatMinutes, pointInformationDMS.LatSeconds,
                                                    pointInformationDMS.LonDegrees, pointInformationDMS.LonMinutes, pointInformationDMS.LonSeconds)
                                                    .ToString("", new CoordinateDMSFormatter());
                    }
                    else
                    {
                        result = new CoordinateDDM(new CoordinateDD(point.Y, point.X))
                            .ToString("", new CoordinateDMSFormatter());
                    }
                    break;
                case CoordinateTypes.MGRS:
                    var pointInformationMGRS = (CoordinateMGRS)PointInformation;
                    result = new CoordinateMGRS(pointInformationMGRS.GZD, pointInformationMGRS.GS, pointInformationMGRS.Easting, pointInformationMGRS.Northing)
                                                .ToString();
                    break;
                case CoordinateTypes.USNG:
                    var pointInformationUSNG = (CoordinateUSNG)PointInformation;
                    result = new CoordinateUSNG(pointInformationUSNG.GZD, pointInformationUSNG.GS, pointInformationUSNG.Easting, pointInformationUSNG.Northing)
                                                .ToString();
                    break;
                case CoordinateTypes.UTM:
                    var pointInformationUTM = (CoordinateUTM)PointInformation;
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
            return result;
        }

        private CoordinateType GetCoordinateType(string input, out ESRI.ArcGIS.Geometry.IPoint point, out CoordinateBase pointInformation)
        {
            point = new PointClass();
            var cn = point as IConversionNotation;
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;
            pointInformation = null;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            point.SpatialReference = geographicCS;
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(numSep, ".") : input;

            try
            {
                CoordinateDD dd;
                if (CoordinateDD.TryParse(input, out dd, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1}", dd.Lat, dd.Lon);
                    cn.PutCoordsFromDD(newInput);
                    pointInformation = dd;
                    return CoordinateType.DD;
                }
            }
            catch { }

            try
            {
                CoordinateDDM ddm;
                if (CoordinateDDM.TryParse(input, out ddm, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", ddm.LatDegrees, ddm.LatMinutes, ddm.LonDegrees, ddm.LonMinutes);
                    cn.PutCoordsFromDD(newInput);
                    pointInformation = ddm;
                    return CoordinateType.DDM;
                }
            }
            catch { }

            try
            {
                CoordinateDMS dms;
                if (CoordinateDMS.TryParse(input, out dms, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5}", dms.LatDegrees, dms.LatMinutes, dms.LatSeconds, dms.LonDegrees, dms.LonMinutes, dms.LonSeconds);
                    cn.PutCoordsFromDD(newInput);
                    pointInformation = dms;
                    return CoordinateType.DMS;
                }
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, input);
                return CoordinateType.GARS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeLL, input);
                return CoordinateType.GARS;
            }
            catch { }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, gars.ToString("", new CoordinateGARSFormatter()));
                    pointInformation = gars;
                    return CoordinateType.GARS;
                }
                catch { }
            }

            // mgrs try parse
            CoordinateMGRS mgrs;
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
                CoordinateMGRS.TryParse(input, out mgrs);
                pointInformation = mgrs;
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewStyle);
                CoordinateMGRS.TryParse(input, out mgrs);
                pointInformation = mgrs;
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewWith180InZone01);
                CoordinateMGRS.TryParse(input, out mgrs);
                pointInformation = mgrs;
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldStyle);
                CoordinateMGRS.TryParse(input, out mgrs);
                pointInformation = mgrs;
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldWith180InZone01);
                CoordinateMGRS.TryParse(input, out mgrs);
                pointInformation = mgrs;
                return CoordinateType.MGRS;
            }
            catch { }

            
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    cn.PutCoordsFromMGRS(mgrs.ToString("", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_NewStyle);
                    pointInformation = mgrs;
                    return CoordinateType.MGRS;
                }
                catch { }
            }

            CoordinateUSNG usng;
            try
            {
                cn.PutCoordsFromUSNG(input);
                CoordinateUSNG.TryParse(input, out usng);
                pointInformation = usng;
                return CoordinateType.USNG;
            }
            catch { }

            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    cn.PutCoordsFromUSNG(usng.ToString("", new CoordinateMGRSFormatter()));
                    pointInformation = usng;
                    return CoordinateType.USNG;
                }
                catch { }
            }

            CoordinateUTM utm;
            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMAddSpaces, input);
                CoordinateUTM.TryParse(input, out utm);
                pointInformation = utm;
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, input);
                CoordinateUTM.TryParse(input, out utm);
                pointInformation = utm;
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMUseNS, input);
                CoordinateUTM.TryParse(input, out utm);
                pointInformation = utm;
                return CoordinateType.UTM;
            }
            catch { }

           
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, utm.ToString("", new CoordinateUTMFormatter()));
                    pointInformation = utm;
                    return CoordinateType.UTM;
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
                    IMap map = ((IMxDocument)ArcMap.Application.Document).FocusMap;
                    var sr = map.SpatialReference != null ? map.SpatialReference : ArcMapHelpers.GetSR((int)esriSRProjCS3Type.esriSRProjCS_WGS1984WebMercatorMajorAuxSphere);
                    point.X = Lon;
                    point.Y = Lat;
                    point.SpatialReference = sr;
                    return CoordinateType.DD;
                }
                catch (Exception)
                {
                    // do nothing
                }
            }

            return CoordinateType.Unknown;
        }
    }
}
