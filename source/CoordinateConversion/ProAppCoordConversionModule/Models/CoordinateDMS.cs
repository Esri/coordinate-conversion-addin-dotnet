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

using ProAppCoordConversionModule.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace ProAppCoordConversionModule.Models
{
    public class CoordinateDMS : CoordinateBase
    {
        public CoordinateDMS() { LatDegrees = 40; LatMinutes = 7; LatSeconds = 22.68; LonDegrees = -78; LonMinutes = 27; LonSeconds = 21.27; }

        public CoordinateDMS(int latd, int latm, double lats, int lond, int lonm, double lons)
        {
            LatDegrees = latd;
            LatMinutes = latm;
            LatSeconds = lats;
            LonDegrees = lond;
            LonMinutes = lonm;
            LonSeconds = lons;
        }

        public CoordinateDMS(CoordinateDD dd)
        {
            LatDegrees = (int)Math.Truncate(dd.Lat);
            double latm = Math.Abs(dd.Lat - Math.Truncate(dd.Lat)) * 60.0;
            LatMinutes = (int)Math.Truncate(latm);
            LatSeconds = (latm - LatMinutes) * 60.0;

            LonDegrees = (int)Math.Truncate(dd.Lon);
            double lonm = Math.Abs(dd.Lon - Math.Truncate(dd.Lon)) * 60.0;
            LonMinutes = (int)Math.Truncate(lonm);
            LonSeconds = (lonm - LonMinutes) * 60.0;
        }

        #region Properties

        public int LatDegrees { get; set; }

        public int LatMinutes
        {
            get;
            set;
        }

        public double LatSeconds
        {
            get;
            set;
        }
        public int LonDegrees
        {
            get;
            set;
        }

        public int LonMinutes
        {
            get;
            set;
        }

        public double LonSeconds
        {
            get;
            set;
        }

        #endregion Properties

        public static bool TryParse(string input, out CoordinateDMS dms, bool displayAmbiguousCoordsDlg = false)
        {
            dms = new CoordinateDMS();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(".", numSep) : input;

            Regex regexDMSLat = new Regex(@"^((?<firstPrefix>[\+\-NnSs])?(?<latitudeD>[0-8]?\d|90)[°˚º^~*\s\-_]+(?<latitudeM>[0-5]?\d|\d)['′\s\-_]+(?<latitudeS>([0-5]?\d|\d)([.,:]\d*)?)[\u0022\u00A8\u02DD\s_]*(?<firstSuffix>[\+\-NnSs])?)([,:;\s|\/\\]+)((?<lastPrefix>[\+\-EeWw])?(?<longitudeD>[0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(?<longitudeM>[0-5]\d|\d)['′\s\-_]+(?<longitudeS>([0-5]?\d|\d)([.,:]\d*)?)[\u0022\u00A8\u02DD\s_]*(?<lastSuffix>[\+\-EeWw])?)[\s]*$");
            Regex regexDMSLon = new Regex(@"^((?<firstPrefix>[\+\-EeWw])?(?<longitudeD>[0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(?<longitudeM>[0-5]\d|\d)['′\s\-_]+(?<longitudeS>([0-5]?\d|\d)([.,:]\d*)?)[\u0022\u00A8\u02DD\s_]*(?<firstSuffix>[\+\-EeWw])?)([,:;\s|\/\\]+)((?<lastPrefix>[\+\-NnSs])?(?<latitudeD>[0-8]?\d|90)[°˚º^~*\s\-_]+(?<latitudeM>[0-5]?\d|\d)['′\s\-_]+(?<latitudeS>([0-5]?\d|\d)([.,:]\d*)?)[\u0022\u00A8\u02DD\s_]*(?<lastSuffix>[\+\-NnSs])?)[\s]*$");

            var matchDMSLat = regexDMSLat.Match(input);
            var matchDMSLon = regexDMSLon.Match(input);

            bool blnMatchDMSLat = matchDMSLat.Success;
            int LatDegrees = -1, LonDegrees = -1, LatMinutes = -1, LonMinutes = -1;
            double LatSeconds = -1, LonSeconds = -1;
            Group firstPrefix = null, firstSuffix = null, lastPrefix = null, lastSuffix = null;

            // Ambiguous coordinate, could be both lat/lon && lon/lat
            if (matchDMSLat.Success && matchDMSLat.Length == input.Length && matchDMSLon.Success && matchDMSLon.Length == input.Length)
            {
                if (CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg && displayAmbiguousCoordsDlg)
                {
                    double latValue = -1, longValue = -1;
                    if (matchDMSLat.Success && matchDMSLat.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDMSLat, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                        {
                            latValue = Double.Parse(matchDMSLat.Groups["latitudeD"].Value);
                            longValue = Double.Parse(matchDMSLat.Groups["longitudeD"].Value);
                        }
                    }
                    else if (matchDMSLon.Success && matchDMSLon.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDMSLon, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                        {
                            latValue = Double.Parse(matchDMSLon.Groups["latitudeD"].Value);
                            longValue = Double.Parse(matchDMSLon.Groups["longitudeD"].Value);
                        }
                    }
                    else
                        return false;

                    if (latValue < 90 && longValue < 90)
                        ShowAmbiguousDialog();
                }

                blnMatchDMSLat = CoordinateConversionLibraryConfig.AddInConfig.isLatLong;
            }

            // Lat/Lon
            if (matchDMSLat.Success && matchDMSLat.Length == input.Length && blnMatchDMSLat)
            {
                if (ValidateNumericCoordinateMatch(matchDMSLat, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                {
                    LatDegrees = int.Parse(matchDMSLat.Groups["latitudeD"].Value);
                    LatMinutes = int.Parse(matchDMSLat.Groups["latitudeM"].Value);
                    LatSeconds = double.Parse(matchDMSLat.Groups["latitudeS"].Value);
                    LonDegrees = int.Parse(matchDMSLat.Groups["longitudeD"].Value);
                    LonMinutes = int.Parse(matchDMSLat.Groups["longitudeM"].Value);
                    LonSeconds = double.Parse(matchDMSLat.Groups["longitudeS"].Value);
                    firstPrefix = matchDMSLat.Groups["firstPrefix"];
                    firstSuffix = matchDMSLat.Groups["firstSuffix"];
                    lastPrefix = matchDMSLat.Groups["lastPrefix"];
                    lastSuffix = matchDMSLat.Groups["lastSuffix"];

                    blnMatchDMSLat = true;
                }
                else
                    return false;
            }
            // Lon/Lat
            else if (matchDMSLon.Success && matchDMSLon.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDMSLon, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                {
                    LatDegrees = int.Parse(matchDMSLon.Groups["latitudeD"].Value);
                    LatMinutes = int.Parse(matchDMSLon.Groups["latitudeM"].Value);
                    LatSeconds = double.Parse(matchDMSLon.Groups["latitudeS"].Value);
                    LonDegrees = int.Parse(matchDMSLon.Groups["longitudeD"].Value);
                    LonMinutes = int.Parse(matchDMSLon.Groups["longitudeM"].Value);
                    LonSeconds = double.Parse(matchDMSLon.Groups["longitudeS"].Value);
                    firstPrefix = matchDMSLon.Groups["firstPrefix"];
                    firstSuffix = matchDMSLon.Groups["firstSuffix"];
                    lastPrefix = matchDMSLon.Groups["lastPrefix"];
                    lastSuffix = matchDMSLon.Groups["lastSuffix"];
                }
                else
                    return false;
            }
            else
                return false;

            // Don't allow both prefix and suffix for lat or lon
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
                LatDegrees = Math.Abs(LatDegrees) * -1;
            }

            if ((firstSuffix.Success || firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) ||
                (lastSuffix.Success || lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (lastSuffix.Value.ToUpper().Equals("W") || lastPrefix.Value.ToUpper().Equals("W")))
            {
                LonDegrees = Math.Abs(LonDegrees) * -1;
            }

            if ((firstSuffix.Success || firstPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (firstSuffix.Value.ToUpper().Equals("-") || firstPrefix.Value.ToUpper().Equals("-")))
            {
                if (blnMatchDMSLat)
                    LatDegrees = Math.Abs(LatDegrees) * -1;
                else
                    LonDegrees = Math.Abs(LonDegrees) * -1;
            }

            if ((lastSuffix.Success || lastPrefix.ValidatePrefix(ShowHyphen, ShowPlus)) && (lastSuffix.Value.ToUpper().Equals("-") || lastPrefix.Value.ToUpper().Equals("-")))
            {
                if (blnMatchDMSLat)
                    LonDegrees = Math.Abs(LonDegrees) * -1;
                else
                    LatDegrees = Math.Abs(LatDegrees) * -1;
            }

            dms = new CoordinateDMS(LatDegrees, LatMinutes, LatSeconds, LonDegrees, LonMinutes, LonSeconds);

            return true;
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            var temp = base.ToString(format, formatProvider);

            if (!string.IsNullOrWhiteSpace(temp))
                return temp;

            var sb = new StringBuilder();

            if (format == null)
                format = "DMS";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "DMS":
                    sb.AppendFormat(fi, "{0}° {1}\' {2:#}\" {3}", Math.Abs(this.LatDegrees), this.LatMinutes, this.LatSeconds, this.LatDegrees < 0 ? "S" : "N");
                    sb.AppendFormat(fi, " {0}° {1}\' {2:#}\" {3}", Math.Abs(this.LonDegrees), this.LonMinutes, this.LonSeconds, this.LonDegrees < 0 ? "W" : "E");
                    break;
                default:
                    throw new Exception("CoordinateDMS.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }

    }

    public class CoordinateDMSFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateDMS)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    if (!string.IsNullOrEmpty(CoordinateBase.InputCustomFormat))
                    {
                        return this.Format(CoordinateBase.InputCustomFormat, arg, this);
                    }
                    return this.Format("A0°B0'C0.00\"N X0°Y0'Z0.00\"E", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateDMS;
                    double cnum = coord.LatDegrees;
                    var sb = new StringBuilder();
                    var olist = new List<object>();
                    int latIndex = -1, lonIndex = -1;
                    var closingIndexes = new List<int>();
                    bool startIndexNeeded = false;
                    bool endIndexNeeded = false;
                    int currentIndex = 0;
                    char lastChar = ' ';

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
                            closingIndexes.Add(sb.Length);
                        }

                        switch (c)
                        {
                            case 'A':
                                cnum = coord.LatDegrees;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                if (coord.LatDegrees > 0.0)
                                {
                                    if (CoordinateBase.ShowPlus) sb.Append("+");
                                }
                                else
                                {
                                    if (CoordinateBase.ShowHyphen) sb.Append("-");
                                }
                                latIndex = sb.Length;
                                break;
                            case 'B':
                                cnum = coord.LatMinutes;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'C':
                                cnum = coord.LatSeconds;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'X':
                                cnum = coord.LonDegrees;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                if (coord.LonDegrees > 0.0)
                                {
                                    if (CoordinateBase.ShowPlus & !CoordinateBase.IsOutputInProcess) sb.Append(" +");
                                }
                                else
                                {
                                    if (CoordinateBase.ShowHyphen & !CoordinateBase.IsOutputInProcess) sb.Append(" -");
                                }
                                lonIndex = sb.Length;
                                break;
                            case 'Y':
                                cnum = coord.LonMinutes;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'Z':
                                cnum = coord.LonSeconds;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'N': // N or S
                            case 'S':
                                if (!CoordinateBase.IsOutputInProcess)
                                {
                                    if (coord.LatDegrees > 0)
                                        sb.Append("N"); // do we always want UPPER
                                    else
                                        sb.Append("S");
                                }
                                break;
                            case 'E': // E or W
                            case 'W':
                                if (!CoordinateBase.IsOutputInProcess)
                                {
                                    if (coord.LonDegrees > 0)
                                        sb.Append("E");
                                    else
                                        sb.Append("W");
                                }
                                break;
                            case '+': // show +
                                if ((lastChar == 'A') || (lastChar == 'X') || (lastChar == '-'))
                                { 
                                    if (cnum > 0.0)
                                        sb.Append("+");
                                    break;
                                }
                                else
                                {
                                    sb.Append(c);
                                    break;
                                }
                            case '-': // show -
                                if ((lastChar == 'A') || (lastChar == 'X') || (lastChar == '+'))
                                {
                                    if (cnum < 0.0)
                                        sb.Append("-");
                                    break;
                                }
                                else
                                {
                                    sb.Append(c);
                                    break;
                                }
                            default:
                                sb.Append(c);
                                break;
                        } // switch

                        lastChar = c;
                    } // foreach 

                    if (endIndexNeeded)
                    {
                        sb.Append("}");
                    }
                    PlaceHemiSphereIndicator(format, coord, sb, latIndex, lonIndex, closingIndexes);

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

        private static void PlaceHemiSphereIndicator(string format, CoordinateDMS coord, StringBuilder sb, int latIndex, int lonIndex, List<int> closingIndexes)
        {
            if (lonIndex != -1 && latIndex != -1)
            {
                int lonVal = -1, latVal = -1;
                if (closingIndexes.Where(x => x < lonIndex).Any())
                {
                    lonVal = closingIndexes.Max();
                    latVal = closingIndexes.Where(x => x < lonIndex).Max();
                }
                else
                {
                    lonVal = closingIndexes.Where(x => x < latIndex).Max();
                    latVal = closingIndexes.Max();
                }

                var nextLatChar = sb.ToString().ElementAt(latVal);
                var skipLatChar = ((nextLatChar == '"' & format.Contains('C'))
                    | (nextLatChar == '\'' & format.Contains('B'))
                    | (nextLatChar == '°' & format.Contains('A')));

                var nextLonChar = sb.ToString().ElementAt(lonVal);
                var skipLonChar = ((nextLonChar == '"' & format.Contains('Z'))
                    | (nextLonChar == '\'' & format.Contains('Y'))
                    | (nextLonChar == '°' & format.Contains('X')));
                if (coord.LonDegrees > 0.0)
                {
                    if (CoordinateBase.ShowHemisphere | CoordinateBase.IsOutputInProcess)
                    {
                        sb.Insert(skipLonChar ? lonVal + 1 : lonVal, "E");
                    }
                }
                else
                {
                    if (CoordinateBase.ShowHemisphere | CoordinateBase.IsOutputInProcess)
                    {
                        sb.Insert(skipLonChar ? lonVal + 1 : lonVal, "W");
                    }
                }
                if (coord.LatDegrees > 0.0)
                {
                    if (CoordinateBase.ShowHemisphere | CoordinateBase.IsOutputInProcess)
                    {
                        sb.Insert(skipLatChar ? latVal + 1 : latVal, "N");
                    }
                }
                else
                {
                    if (CoordinateBase.ShowHemisphere | CoordinateBase.IsOutputInProcess)
                    {
                        sb.Insert(skipLatChar ? latVal + 1 : latVal, "S");
                    }
                }
            }
        }
    }
}
