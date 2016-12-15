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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateDMS : CoordinateBase
    {
        public CoordinateDMS() { LatDegrees = 40; LatMinutes = 7; LatSeconds = 22.8; LonDegrees = -78; LonMinutes = 27; LonSeconds = 21.6; }

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

        public static bool TryParse(string input, out CoordinateDMS dms)
        {
            dms = new CoordinateDMS();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(".", numSep) : input;

            Regex regexDMS = new Regex(@"^ *[+]*(?<firstPrefix>[NSEW])?(?<latitudeD>((-| )|)\d+)[° ]*(?<latitudeM>\d+)[' ]*(?<latitudeS>\d+[.,]\d+)[""]?(?<firstSuffix>[NSEW]?)([, +]*)(?<lastPrefix>[NSEW])?(?<longitudeD>((-| )|)\d+)[° ]*(?<longitudeM>\d+)[' ]*(?<longitudeS>\d+[.,]\d+)[""]?(?<lastSuffix>[NSEW]?)", RegexOptions.ExplicitCapture);

            var matchDMS = regexDMS.Match(input);

            if (matchDMS.Success && matchDMS.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDMS, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                {
                    try
                    {
                        var firstPrefix = matchDMS.Groups["firstPrefix"];
                        var firstSuffix = matchDMS.Groups["firstSuffix"];
                        var lastPrefix = matchDMS.Groups["lastPrefix"];
                        var lastSuffix = matchDMS.Groups["lastSuffix"];

                        // Don't allow both prefix and suffix for lat or lon
                        if (firstPrefix.Success && firstSuffix.Success)
                        {
                            return false;
                        }

                        if (lastPrefix.Success && lastSuffix.Success)
                        {
                            return false;
                        }

                        // Don't allow same prefix/suffix for both lat and lon
                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("E") || firstPrefix.Value.ToUpper().Equals("E") ||
                                                                           firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) &&
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("E") || lastPrefix.Value.ToUpper().Equals("E") ||
                                                                           lastSuffix.Value.ToUpper().Equals("W") || lastPrefix.Value.ToUpper().Equals("W")))
                        {
                            return false;
                        }

                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("N") || firstPrefix.Value.ToUpper().Equals("N") ||
                                                                           firstSuffix.Value.ToUpper().Equals("S") || firstPrefix.Value.ToUpper().Equals("S")) &&
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("N") || lastPrefix.Value.ToUpper().Equals("N") ||
                                                                           lastSuffix.Value.ToUpper().Equals("S") || lastPrefix.Value.ToUpper().Equals("S")))
                        {
                            return false;
                        }

                        var LatDegrees = int.Parse(matchDMS.Groups["latitudeD"].Value);
                        var LatMinutes = int.Parse(matchDMS.Groups["latitudeM"].Value);
                        var LatSeconds = double.Parse(matchDMS.Groups["latitudeS"].Value);
                        var LonDegrees = int.Parse(matchDMS.Groups["longitudeD"].Value);
                        var LonMinutes = int.Parse(matchDMS.Groups["longitudeM"].Value);
                        var LonSeconds = double.Parse(matchDMS.Groups["longitudeS"].Value);

                        // if E/W is in first coordinate or N/S is in second coordinate then flip the lat/lon values
                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("E") || firstPrefix.Value.ToUpper().Equals("E") ||
                                                                           firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) ||
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("N") || lastPrefix.Value.ToUpper().Equals("N") ||
                                                                           lastSuffix.Value.ToUpper().Equals("S") || lastPrefix.Value.ToUpper().Equals("S")))
                        {
                            LatDegrees = int.Parse(matchDMS.Groups["longitudeD"].Value);
                            LatMinutes = int.Parse(matchDMS.Groups["longitudeM"].Value);
                            LatSeconds = double.Parse(matchDMS.Groups["longitudeS"].Value);
                            LonDegrees = int.Parse(matchDMS.Groups["latitudeD"].Value);
                            LonMinutes = int.Parse(matchDMS.Groups["latitudeM"].Value);
                            LonSeconds = double.Parse(matchDMS.Groups["latitudeS"].Value);
                        }

                        // no suffix or prefix was added so allow user to specify longitude first by checking for absolute value greater than 90
                        // fix for bug Bob Booth found in issue #42
                        if (!firstPrefix.Success && !firstSuffix.Success && !lastPrefix.Success && !lastSuffix.Success)
                        {
                            // switch the values if longitude was added first
                            if ((Math.Abs(LatDegrees) > 90.0) && (Math.Abs(LonDegrees) <= 90.0))
                            {
                                LatDegrees = int.Parse(matchDMS.Groups["longitudeD"].Value);
                                LatMinutes = int.Parse(matchDMS.Groups["longitudeM"].Value);
                                LatSeconds = double.Parse(matchDMS.Groups["longitudeS"].Value);
                                LonDegrees = int.Parse(matchDMS.Groups["latitudeD"].Value);
                                LonMinutes = int.Parse(matchDMS.Groups["latitudeM"].Value);
                                LonSeconds = double.Parse(matchDMS.Groups["latitudeS"].Value);
                            }
                            if ((Math.Abs(LatDegrees) > 90.0) && (Math.Abs(LonDegrees) > 90.0))
                            {
                                return false;
                            }
                        }

                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("S") || firstPrefix.Value.ToUpper().Equals("S")) ||
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("S") || lastPrefix.Value.ToUpper().Equals("S")))
                        {
                            LatDegrees = Math.Abs(LatDegrees) * -1;
                        }

                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) ||
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("W") || lastPrefix.Value.ToUpper().Equals("W")))
                        {
                            LonDegrees = Math.Abs(LonDegrees) * -1;
                        }
                        if ((Math.Abs(LatDegrees) > 90.0) && (Math.Abs(LonDegrees) > 90.0))
                        {
                            return false;
                        }
                        dms = new CoordinateDMS(LatDegrees, LatMinutes, LatSeconds, LonDegrees, LonMinutes, LonSeconds);
                    }
                    catch
                    {
                        return false;
                    }

                    return true;
                }
            }
            return false;
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
                    return this.Format("A0°B0'C0.00\"N X0°Y0'Z0.00\"E", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateDMS;
                    double cnum = coord.LatDegrees;
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
                            case 'A':
                                cnum = coord.LatDegrees;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
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
                            case '+': // show + or -
                                if (cnum > 0.0)
                                    sb.Append("+");
                                break;
                            case '-':
                                if (cnum < 0.0)
                                    sb.Append("-");
                                break;
                            case 'N': // N or S
                            case 'S':
                                if (coord.LatDegrees > 0)
                                    sb.Append("N"); // do we always want UPPER
                                else
                                    sb.Append("S");
                                break;
                            case 'E': // E or W
                            case 'W':
                                if (coord.LonDegrees > 0)
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
                        endIndexNeeded = false;
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
