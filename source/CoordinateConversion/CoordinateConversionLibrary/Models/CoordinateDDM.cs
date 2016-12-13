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
    public class CoordinateDDM : CoordinateBase
    {
        public CoordinateDDM() { LatDegrees = 40; LatMinutes = 7.38; LonDegrees = -78; LonMinutes = 27.36; }

        public CoordinateDDM(int latd, double latm, int lond, double lonm)
        {
            LatDegrees = latd;
            LatMinutes = latm;
            LonDegrees = lond;
            LonMinutes = lonm;
        }

        public CoordinateDDM(CoordinateDD dd)
        {
            LatDegrees = (int)Math.Truncate(dd.Lat);
            LatMinutes = Math.Abs(dd.Lat - Math.Truncate(dd.Lat)) * 60.0;
            LonDegrees = (int)Math.Truncate(dd.Lon);
            LonMinutes = Math.Abs(dd.Lon - Math.Truncate(dd.Lon)) * 60.0;
        }

        #region Properties

        public int LatDegrees { get; set; }
        public double LatMinutes { get; set; }
        public int LonDegrees { get; set; }
        public double LonMinutes { get; set; }

        #endregion Properties

        public static bool TryParse(string input, out CoordinateDDM ddm)
        {
            ddm = new CoordinateDDM();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            Regex regexDDM = new Regex(@"^\s*[+]*(?<firstPrefix>[NSEW])?(?<latitudeD>((-| )|(?=\d))\d+)[° ]?(?<latitudeM>\d+[.,:\s]\d+)(?<firstSuffix>[NSEW])?[',\s]*(?<lastPrefix>[NSEW])?(?<longitudeD>((-| )|(?=\d))\d+)[° ]?(?<longitudeM>\d+[.,:\s]\d+)?[',:\s]*(?<lastSuffix>[NSEW])?");

            var matchDDM = regexDDM.Match(input);

            if (matchDDM.Success && matchDDM.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDDM, new string[] { "latitudeD", "latitudeM", "longitudeD", "longitudeM" }))
                {
                    try
                    {
                        var firstPrefix = matchDDM.Groups["firstPrefix"];
                        var firstSuffix = matchDDM.Groups["firstSuffix"];
                        var lastPrefix = matchDDM.Groups["lastPrefix"];
                        var lastSuffix = matchDDM.Groups["lastSuffix"];

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

                        var LatDegrees = int.Parse(matchDDM.Groups["latitudeD"].Value);
                        var LatMinutes = double.Parse(matchDDM.Groups["latitudeM"].Value);
                        var LonDegrees = int.Parse(matchDDM.Groups["longitudeD"].Value);
                        var LonMinutes = double.Parse(matchDDM.Groups["longitudeM"].Value);

                        // if E/W is in first coordinate or N/S is in second coordinate then flip the lat/lon values
                        if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("E") || firstPrefix.Value.ToUpper().Equals("E") ||
                                                                           firstSuffix.Value.ToUpper().Equals("W") || firstPrefix.Value.ToUpper().Equals("W")) ||
                            (lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("N") || lastPrefix.Value.ToUpper().Equals("N") ||
                                                                           lastSuffix.Value.ToUpper().Equals("S") || lastPrefix.Value.ToUpper().Equals("S")))
                        {
                            LatDegrees = int.Parse(matchDDM.Groups["longitudeD"].Value);
                            LatMinutes = double.Parse(matchDDM.Groups["longitudeM"].Value);
                            LonDegrees = int.Parse(matchDDM.Groups["latitudeD"].Value);
                            LonMinutes = double.Parse(matchDDM.Groups["latitudeM"].Value);
                        }

                        // no suffix or prefix was added so allow user to specify longitude first by checking for absolute value greater than 90
                        // fix for bug Bob Booth found in issue #42
                        if (!firstPrefix.Success && !firstSuffix.Success && !lastPrefix.Success && !lastSuffix.Success)
                        {
                            // switch the values if longitude was added first
                            if ((Math.Abs(LatDegrees) > 90.0) && (Math.Abs(LonDegrees) <= 90.0))
                            {
                                LatDegrees = int.Parse(matchDDM.Groups["longitudeD"].Value);
                                LatMinutes = double.Parse(matchDDM.Groups["longitudeM"].Value);
                                LonDegrees = int.Parse(matchDDM.Groups["latitudeD"].Value);
                                LonMinutes = double.Parse(matchDDM.Groups["latitudeM"].Value);
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

                        ddm = new CoordinateDDM(LatDegrees, LatMinutes, LonDegrees, LonMinutes);
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
                format = "DDM";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "DDM":
                    sb.AppendFormat(fi, "{0}° {1:0.0#####}\' {3}", Math.Abs(this.LatDegrees), this.LatMinutes, this.LatDegrees < 0 ? "S" : "N");
                    sb.AppendFormat(fi, " {0}° {1:0.0#####}\' {3}", Math.Abs(this.LonDegrees), this.LonMinutes, this.LonDegrees < 0 ? "W" : "E");
                    break;
                default:
                    throw new Exception("CoordinateDDM.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }
    }

    public class CoordinateDDMFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateDDM)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    return this.Format("A-0°B0.0000' X-0°Y0.0000'", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateDDM;
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
