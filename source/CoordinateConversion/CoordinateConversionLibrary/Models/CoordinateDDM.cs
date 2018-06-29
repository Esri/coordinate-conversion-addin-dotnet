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

using CoordinateConversionLibrary.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateDDM : CoordinateBase
    {
        public CoordinateDDM() { LatDegrees = 40; LatMinutes = 7.4876; LonDegrees = -78; LonMinutes = 27.3292; }

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

        public static bool TryParse(string input, out CoordinateDDM ddm, bool displayAmbiguousCoordsDlg = false)
        {
            ddm = new CoordinateDDM();

            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(".", numSep) : input;

            Regex regexDDMLat = new Regex(@"^((?<firstPrefix>[\+\-NnSs])?(?<latitudeD>[0-8]?\d|90)[°˚º^~*\s\-_]+(?<latitudeM>([0-5]?\d|\d)([.,:]\d*)?)['′\s_]*(?<firstSuffix>[\+\-NnSs])?)([,:;\s|\/\\]+)((?<lastPrefix>[\+\-EeWw])?(?<longitudeD>[0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(?<longitudeM>([0-5]\d|\d)([.,:]\d*)?)['′\s_]*(?<lastSuffix>[\+\-EeWw])?)[\s]*$");
            Regex regexDDMLon = new Regex(@"^((?<firstPrefix>[\+\-EeWw])?(?<longitudeD>[0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(?<longitudeM>([0-5]\d|\d)([.,:]\d*)?)['′\s_]*(?<firstSuffix>[\+\-EeWw])?)([,:;\s|\/\\]+)((?<lastPrefix>[\+\-NnSs])?(?<latitudeD>[0-8]?\d|90)[°˚º^~*\s\-_]+(?<latitudeM>([0-5]?\d|\d)([.,:]\d*)?)['′\s_]*(?<lastSuffix>[\+\-NnSs])?)[\s]*$");

            var matchDDMLat = regexDDMLat.Match(input);
            var matchDDMLon = regexDDMLon.Match(input);

            bool blnMatchDDMLat = matchDDMLat.Success;
            int LatDegrees = -1, LonDegrees = -1;
            double LatMinutes = -1, LonMinutes = -1;
            Group firstPrefix = null, firstSuffix = null, lastPrefix = null, lastSuffix = null;

            // Ambiguous coordinate, could be both lat/lon && lon/lat
            if (matchDDMLat.Success && matchDDMLat.Length == input.Length && matchDDMLon.Success && matchDDMLon.Length == input.Length)
            {
                if (CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg && displayAmbiguousCoordsDlg)
                {
                    double latValue = -1, longValue = -1;
                    if (matchDDMLat.Success && matchDDMLat.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDDMLat, new string[] { "latitudeD", "latitudeM", "longitudeD", "longitudeM" }))
                        {
                            latValue = Double.Parse(matchDDMLat.Groups["latitude"].Value);
                            longValue = Double.Parse(matchDDMLat.Groups["longitude"].Value);
                        }
                    }
                    else if (matchDDMLon.Success && matchDDMLon.Length == input.Length)
                    {
                        if (ValidateNumericCoordinateMatch(matchDDMLon, new string[] { "latitudeD", "latitudeM", "longitudeD", "longitudeM" }))
                        {
                            latValue = Double.Parse(matchDDMLon.Groups["latitudeD"].Value);
                            longValue = Double.Parse(matchDDMLon.Groups["longitudeD"].Value);
                        }
                    }
                    else
                        return false;

                    if (latValue < 90 && longValue < 90)
                        ambiguousCoordsViewDlg.ShowDialog();
                }

                blnMatchDDMLat = ambiguousCoordsViewDlg.CheckedLatLon;
            }

            // Lat/Lon
            if (matchDDMLat.Success && matchDDMLat.Length == input.Length && blnMatchDDMLat)
            {
                if (ValidateNumericCoordinateMatch(matchDDMLat, new string[] { "latitudeD", "latitudeM", "longitudeD", "longitudeM" }))
                {
                    LatDegrees = int.Parse(matchDDMLat.Groups["latitudeD"].Value);
                    LatMinutes = double.Parse(matchDDMLat.Groups["latitudeM"].Value);
                    LonDegrees = int.Parse(matchDDMLat.Groups["longitudeD"].Value);
                    LonMinutes = double.Parse(matchDDMLat.Groups["longitudeM"].Value);
                    firstPrefix = matchDDMLat.Groups["firstPrefix"];
                    firstSuffix = matchDDMLat.Groups["firstSuffix"];
                    lastPrefix = matchDDMLat.Groups["lastPrefix"];
                    lastSuffix = matchDDMLat.Groups["lastSuffix"];

                    blnMatchDDMLat = true;
                }
                else
                    return false;
            }
            // Lon/Lat
            else if (matchDDMLon.Success && matchDDMLon.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDDMLon, new string[] { "latitudeD", "latitudeM", "longitudeD", "longitudeM" }))
                {
                    LatDegrees = int.Parse(matchDDMLon.Groups["latitudeD"].Value);
                    LatMinutes = double.Parse(matchDDMLon.Groups["latitudeM"].Value);
                    LonDegrees = int.Parse(matchDDMLon.Groups["longitudeD"].Value);
                    LonMinutes = double.Parse(matchDDMLon.Groups["longitudeM"].Value);
                    firstPrefix = matchDDMLon.Groups["firstPrefix"];
                    firstSuffix = matchDDMLon.Groups["firstSuffix"];
                    lastPrefix = matchDDMLon.Groups["lastPrefix"];
                    lastSuffix = matchDDMLon.Groups["lastSuffix"];
                }
                else
                    return false;
            }
            else
                return false;

            // Don't allow both prefix and suffix for lat or lon
            if (firstPrefix.Success && firstSuffix.Success)
            {
                return false;
            }

            if (lastPrefix.Success && lastSuffix.Success)
            {
                return false;
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

            if ((firstSuffix.Success || firstPrefix.Success) && (firstSuffix.Value.ToUpper().Equals("-") || firstPrefix.Value.ToUpper().Equals("-")))
            {
                if (blnMatchDDMLat)
                    LatDegrees = Math.Abs(LatDegrees) * -1;
                else
                    LonDegrees = Math.Abs(LonDegrees) * -1;
            }

            if ((lastSuffix.Success || lastPrefix.Success) && (lastSuffix.Value.ToUpper().Equals("-") || lastPrefix.Value.ToUpper().Equals("-")))
            {
                if (blnMatchDDMLat)
                    LonDegrees = Math.Abs(LonDegrees) * -1;
                else
                    LatDegrees = Math.Abs(LatDegrees) * -1;
            }

            ddm = new CoordinateDDM(LatDegrees, LatMinutes, LonDegrees, LonMinutes);

            return true;
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
                    sb.AppendFormat(fi, "{0}° {1:0.0#####}\' {2}", Math.Abs(this.LatDegrees), this.LatMinutes, this.LatDegrees < 0 ? "S" : "N");
                    sb.AppendFormat(fi, " {0}° {1:0.0#####}\' {2}", Math.Abs(this.LonDegrees), this.LonMinutes, this.LonDegrees < 0 ? "W" : "E");
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
