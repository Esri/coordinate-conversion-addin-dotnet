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

using CoordinateConversionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateDD : CoordinateBase
    {
        public CoordinateDD() { Lat = 40.378465; Lon = -78.456799; }

        public CoordinateDD(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public CoordinateDD(CoordinateDDM ddm)
        {
            Lat = (Math.Abs((double)ddm.LatDegrees) + (ddm.LatMinutes / 60.0)) * ((ddm.LatDegrees < 0) ? -1.0 : 1.0);
            Lon = (Math.Abs((double)ddm.LonDegrees) + (ddm.LonMinutes / 60.0)) * ((ddm.LonDegrees < 0) ? -1.0 : 1.0);
        }

        public CoordinateDD(CoordinateDMS dms)
        {
            Lat = (Math.Abs((double)dms.LatDegrees) + ((double)dms.LatMinutes / 60.0) + (dms.LatSeconds / 3600.0)) * ((dms.LatDegrees < 0) ? -1.0 : 1.0);
            Lon = (Math.Abs((double)dms.LonDegrees) + ((double)dms.LonMinutes / 60.0) + (dms.LonSeconds / 3600.0)) * ((dms.LonDegrees < 0) ? -1.0 : 1.0);
        }

        #region Properties

        private double _lat = 40.446;
        private double _lon = -79.982;

        public double Lat
        {
            get { return _lat; }
            set { _lat = value; }
        }

        public double Lon
        {
            get { return _lon; }
            set { _lon = value; }
        }

        #endregion Properties

        #region Methods

        public static bool TryParse(string input, out CoordinateDD coord, bool displayAmbiguousCoordsDlg = false)
        {
            coord = new CoordinateDD();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(".", numSep) : input;

            Regex regexDDLat = new Regex(@"^((?<firstPrefix>[NnSs\+-])?(?<latitude>[0-8]?\d([,.:]\d*)?|90([,.:]0*)?)([°˚º^~*\s]*)(?<firstSuffix>[NnSs\+-])?)([,:;\s|\/\\]+)((?<lastPrefix>[EeWw\+-])?(?<longitude>[0]?\d?\d([,.:]\d*)?|1[0-7]\d([,.:]\d*)?|180([,.:]0*)?)([°˚º^~*\s]*)(?<lastSuffix>[EeWw\+-])?)$");
            Regex regexDDLon = new Regex(@"^((?<firstPrefix>[EeWw\+-])?(?<longitude>[0]?\d?\d([,.:]\d*)?|1[0-7]\d([,.:]\d*)?|180([,.:]0*)?)([°˚º^~*\s]*)(?<firstSuffix>[EeWw\+-])?)([,:;\s|\/\\]+)((?<lastPrefix>[NnSs\+-])?(?<latitude>[0-8]?\d?\d([,.:]\d*)?|90([,.:]0*)?)([°˚º^~*\s]*)(?<lastSuffix>[NnSs\+-])?)$");

            var matchDDLat = regexDDLat.Match(input);
            var matchDDLon = regexDDLon.Match(input);

            bool blnMatchDDLat = matchDDLat.Success;
            double latitude = -1, longitude = -1;
            Group firstPrefix = null, firstSuffix = null, lastPrefix = null, lastSuffix = null;

            // Ambiguous coordinate, could be both lat/lon && lon/lat
            if (matchDDLat.Success && matchDDLat.Length == input.Length && matchDDLon.Success && matchDDLon.Length == input.Length)
            {
                if (CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg && displayAmbiguousCoordsDlg)
                {
                    double latValue = -1, longValue = -1;
                    if (matchDDLat.Success && matchDDLat.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDDLat, new string[] { "latitude", "longitude" }))
                        {
                            latValue = Double.Parse(matchDDLat.Groups["latitude"].Value);
                            longValue = Double.Parse(matchDDLat.Groups["longitude"].Value);
                        }
                    }
                    else if (matchDDLon.Success && matchDDLon.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDDLon, new string[] { "latitude", "longitude" }))
                        {
                            latValue = Double.Parse(matchDDLon.Groups["latitude"].Value);
                            longValue = Double.Parse(matchDDLon.Groups["longitude"].Value);
                        }
                    }
                    else
                        return false;

                    if (latValue < 90 && longValue < 90)
                        ShowAmbiguousDialog();
                }
                blnMatchDDLat = CoordinateConversionLibraryConfig.AddInConfig.isLatLong;
            }

            // Lat/Lon
            if (matchDDLat.Success && matchDDLat.Length == input.Length && blnMatchDDLat)
            {
                if (ValidateNumericCoordinateMatch(matchDDLat, new string[] { "latitude", "longitude" }))
                {
                    latitude = Double.Parse(matchDDLat.Groups["latitude"].Value);
                    longitude = Double.Parse(matchDDLat.Groups["longitude"].Value);
                    firstPrefix = matchDDLat.Groups["firstPrefix"];
                    firstSuffix = matchDDLat.Groups["firstSuffix"];
                    lastPrefix = matchDDLat.Groups["lastPrefix"];
                    lastSuffix = matchDDLat.Groups["lastSuffix"];
                }
            }
            // Lon/Lat
            else if (matchDDLon.Success && matchDDLon.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDDLon, new string[] { "latitude", "longitude" }))
                {
                    latitude = Double.Parse(matchDDLon.Groups["latitude"].Value);
                    longitude = Double.Parse(matchDDLon.Groups["longitude"].Value);
                    firstPrefix = matchDDLon.Groups["firstPrefix"];
                    firstSuffix = matchDDLon.Groups["firstSuffix"];
                    lastPrefix = matchDDLon.Groups["lastPrefix"];
                    lastSuffix = matchDDLon.Groups["lastSuffix"];
                }
            }
            else
                return false;

            coord.Lat = latitude;
            coord.Lon = longitude;


            try
            {
                //// Don't allow both prefix and suffix for lat or lon
                if (firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus) && firstSuffix.Success)
                {
                    return false;
                }

                if (lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus) && lastSuffix.Success)
                {
                    return false;
                }

                if ((firstSuffix.Success || firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (firstSuffix.Value.ToUpper().Equals("S") || firstPrefix.Value.ToUpper().Equals("S")) ||
                    (lastSuffix.Success || lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (lastSuffix.Value.ToUpper().Equals("S") || lastPrefix.Value.ToUpper().Equals("S")))
                {
                    coord.Lat = Math.Abs(coord.Lat) * -1;
                }

                if ((firstSuffix.Success || firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) ||
                    (lastSuffix.Success || lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (lastSuffix.Value.ToUpper().Equals("W") || lastPrefix.Value.ToUpper().Equals("W")))
                {
                    coord.Lon = Math.Abs(coord.Lon) * -1;
                }

                if ((firstSuffix.Success || firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (firstSuffix.Value.ToUpper().Equals("-") || firstPrefix.Value.ToUpper().Equals("-")))
                {
                    if (blnMatchDDLat)
                        coord.Lat = Math.Abs(coord.Lat) * -1;
                    else
                        coord.Lon = Math.Abs(coord.Lon) * -1;
                }

                if ((lastSuffix.Success || lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (lastSuffix.Value.ToUpper().Equals("-") || lastPrefix.Value.ToUpper().Equals("-")))
                {
                    if (blnMatchDDLat)
                        coord.Lon = Math.Abs(coord.Lon) * -1;
                    else
                        coord.Lat = Math.Abs(coord.Lat) * -1;
                }

            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion Methods

        #region ToString

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            var temp = base.ToString(format, formatProvider);

            if (!string.IsNullOrWhiteSpace(temp))
                return temp;

            var sb = new StringBuilder();

            if (format == null)
                format = "DD";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "DD":
                    sb.AppendFormat(fi, "x = {0:0.0000##}", this.Lon);
                    sb.AppendFormat(fi, " y = {0:0.0000##}", this.Lat);
                    break;
                default:
                    throw new Exception("CoordinateDD.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }

        #endregion ToString
    }

    public class CoordinateDDFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateDD)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    if (!string.IsNullOrEmpty(CoordinateBase.InputCustomFormat))
                    {
                        return this.Format(CoordinateBase.InputCustomFormat, arg, this);
                    }
                    return this.Format("Y-0.000000 X-0.000000", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateDD;
                    var cnum = coord.Lat;
                    var sb = new StringBuilder();
                    var olist = new List<object>();
                    bool startIndexNeeded = false;
                    bool endIndexNeeded = false;
                    int currentIndex = 0;

                    foreach (char c in format)
                    {
                        if (startIndexNeeded && (c == '#' || c == '.' || c == '0'))
                        {
                            // add {<index>:
                            sb.AppendFormat("{{{0}:", currentIndex++);
                            startIndexNeeded = false;
                            endIndexNeeded = true;
                        }

                        if (endIndexNeeded && (c != '#' && c != '.' && c != '0'))
                        {
                            sb.Append("}");
                            endIndexNeeded = false;
                        }

                        switch (c)
                        {
                            case 'X': // longitude coordinate
                                cnum = coord.Lon;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                if (coord.Lon > 0.0)
                                {
                                    if (CoordinateBase.ShowPlus) sb.Append("+");
                                }

                                else
                                {
                                    if (CoordinateBase.ShowHyphen) sb.Append("-");
                                }
                                break;
                            case 'Y': // latitude coordinate
                                cnum = coord.Lat;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                if (coord.Lat > 0.0)
                                {
                                    if (CoordinateBase.ShowPlus) sb.Append(" +");
                                }

                                else
                                {
                                    if (CoordinateBase.ShowHyphen) sb.Append(" -");
                                }
                                break;
                            case '+': // show +
                                if (cnum > 0.0)
                                    sb.Append("+");
                                break;
                            case '-': // show -
                                if (cnum < 0.0)
                                    sb.Append("-");
                                break;
                            case 'N':
                            case 'S': // N or S
                                if (coord.Lat > 0.0)
                                    sb.Append("N"); // do we always want UPPER
                                else
                                    sb.Append("S");
                                break;
                            case 'E':
                            case 'W': // E or W
                                if (coord.Lon > 0.0)
                                    sb.Append("E");
                                else
                                    sb.Append("W");
                                break;
                            default:
                                sb.Append(c);
                                break;
                        }
                    }

                    if (endIndexNeeded)
                    {
                        sb.Append("}");
                    }

                    return String.Format(sb.ToString(), olist.ToArray());

                }
            }

            if (arg is IFormattable)
            {
                return ((IFormattable)arg).ToString(format, formatProvider);
            }
            else
            {
                return arg.ToString();
            }
        }
    }
}
