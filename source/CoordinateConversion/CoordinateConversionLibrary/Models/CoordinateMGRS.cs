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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateMGRS : CoordinateBase
    {
        public CoordinateMGRS() 
        {
            GZD = "17T";
            GS = "QQ";
            Easting = 16777;
            Northing = 44511;
        }

        // grid zone, grid square, easting, northing 5 digits is 1m
        public CoordinateMGRS(string gzd, string gsquare, int easting, int northing)
        {
            GZD = gzd;
            GS = gsquare;
            Northing = northing;
            Easting = easting;
        }

        public string GZD { get; set; }
        public string GS { get; set; }
        public int Easting { get; set; }
        public int Northing { get; set; }

        #region Methods

        public static bool TryParse(string input, out CoordinateMGRS mgrs)
        {
            mgrs = new CoordinateMGRS();

            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            input = input.Trim();

            Regex regexMGRS = new Regex(@"^\s*(?<gzd>\d{1,2}[C-HJ-NP-X])[-,;:\s]*(?<gs1>[A-HJ-NP-Z]{1})(?<gs2>[A-HJ-NP-V]{1})[-,;:\s]*(?<numlocation>\d{0,10})[-,;:\s]*(?<numlocation2>\d{0,10})\s*");

            var matchMGRS = regexMGRS.Match(input);

            if(matchMGRS.Success && matchMGRS.Length == input.Length)
            {
                //if (ValidateNumericCoordinateMatch(matchMGRS, new string[] { "numlocation", "numlocation2" }))
                {
                    // need to validate the gzd and gs
                    try
                    {
                        mgrs.GZD = matchMGRS.Groups["gzd"].Value;
                        mgrs.GS = string.Format("{0}{1}",matchMGRS.Groups["gs1"].Value,matchMGRS.Groups["gs2"].Value);
                        var tempEN = string.Format("{0}{1}",matchMGRS.Groups["numlocation"].Value,matchMGRS.Groups["numlocation2"].Value);

                        if (tempEN.Length % 2 == 0 && tempEN.Length > 0)
                        {
                            int numSize = tempEN.Length / 2;
                            mgrs.Easting = Int32.Parse(tempEN.Substring(0, numSize));
                            mgrs.Northing = Int32.Parse(tempEN.Substring(numSize, numSize));
                        }
                        else
                        {
                            mgrs.Easting = 0;
                            mgrs.Northing = 0;
                        }
                    }
                    catch
                    {
                        return false;
                    }

                    return Validate(mgrs);
                }
            }

            return false;
        }

        public static bool Validate(CoordinateMGRS mgrs)
        {
            try
            {
                var zone = Convert.ToInt32(mgrs.GZD.Substring(0, mgrs.GZD.Length - 1));
                if (zone < 1 || zone > 60)
                    return false;
            }
            catch { return false; }

            return true;
        }

        #endregion

        #region ToString

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            var temp = base.ToString(format, formatProvider);

            if (!string.IsNullOrWhiteSpace(temp))
                return temp;

            var sb = new StringBuilder();

            if (format == null)
                format = "MGRS";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "MGRS":
                    sb.Append(GZD);
                    sb.Append(GS);
                    sb.AppendFormat(fi, "{0:00000}", this.Easting);
                    sb.AppendFormat(fi, "{0:00000}", this.Northing);
                    //sb.Append(this.Easting.ToString().PadRight(5, '0'));
                    //sb.Append(this.Northing.ToString().PadRight(5, '0'));
                    break;
                default:
                    throw new Exception("CoordinateMGRS.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }

        #endregion
    }

    public class CoordinateMGRSFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateMGRS)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    return this.Format("ZSX00000Y00000", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateMGRS;
                    var cnum = coord.Easting;
                    var sb = new StringBuilder();
                    var olist = new List<object>();
                    bool startIndexNeeded = false;
                    bool endIndexNeeded = false;
                    int currentIndex = 0;

                    foreach (char c in format)
                    {
                        if (startIndexNeeded && (c == '#' || c == '0'))
                        {
                            // add {<index>:
                            sb.AppendFormat("{{{0}:", currentIndex++);
                            startIndexNeeded = false;
                            endIndexNeeded = true;
                        }

                        if (endIndexNeeded && (c != '#' && c != '0'))
                        {
                            sb.Append("}");
                            endIndexNeeded = false;
                        }

                        switch (c)
                        {
                            case 'X': // easting
                                cnum = coord.Easting;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'Y': // northing
                                cnum = coord.Northing;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'Z': // grid zone
                                sb.Append(coord.GZD);
                                break;
                            case 'S': // grid segment
                                sb.Append(coord.GS);
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
