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

namespace ProAppCoordConversionModule.Models
{
    public class CoordinateGARS : CoordinateBase
    {
        public CoordinateGARS()
        {
            LonBand = 204;
            LatBand = "LW";
            Quadrant = 3;
            Key = 4;
        }

        public CoordinateGARS(int lonBand, string latBand, int quadrant, int key)
        {
            LonBand = lonBand;
            LatBand = latBand;
            Quadrant = quadrant;
            Key = key;
        }

        public int LonBand { get; set; }
        public string LatBand { get; set; }
        public int Quadrant { get; set; }
        public int Key { get; set; }

        #region Methods

        public static bool TryParse(string input, out CoordinateGARS gars)
        {
            gars = new CoordinateGARS();

            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            input = input.Trim();

            Regex regexGARS = new Regex(@"^\s*(?<lonband>\d{3})[-,;:\s]*(?<latband1>[A-HJ-NP-Q]{1}?)(?<latband2>[A-HJ-NP-Z]{1}?)[-,;:\s]*(?<quadrant>\d?)[-,;:\s]*(?<key>\d?)\s*");

            var matchGARS = regexGARS.Match(input);

            if (matchGARS.Success && matchGARS.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchGARS, new string[] { "lonband", "quadrant", "key" }))
                {
                    // need to validate the latband
                    try
                    {
                        gars.LonBand = Int32.Parse(matchGARS.Groups["lonband"].Value);
                        gars.LatBand = string.Format("{0}{1}",matchGARS.Groups["latband1"].Value,matchGARS.Groups["latband2"].Value);
                        gars.Quadrant = Int32.Parse(matchGARS.Groups["quadrant"].Value);
                        gars.Key = Int32.Parse(matchGARS.Groups["key"].Value);
                    }
                    catch
                    {
                        /* Conversion Failed */
                        return false;
                    }

                    return Validate(gars);
                }
            }

            return false;
        }

        public static bool Validate(CoordinateGARS gars)
        {
            if (gars.LonBand < 1 || gars.LonBand > 720)
                return false;

            Regex regexGARS = new Regex(@"^\s*(?<latband1>[A-HJ-NP-Q]{1}?)(?<latband2>[A-HJ-NP-Z]{1}?)\s*");

            var matchGARS = regexGARS.Match(gars.LatBand);

            if (!matchGARS.Success)
                return false;

            if (gars.Quadrant < 1 || gars.Quadrant > 4)
                return false;

            if (gars.Key < 1 || gars.Key > 9)
                return false;

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
                format = "GARS";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "GARS":
                    sb.AppendFormat(fi, "{0:000}", LonBand);
                    sb.Append(LatBand);
                    sb.AppendFormat(fi, "{0:#}", Quadrant);
                    sb.AppendFormat(fi, "{0:#}", Key);
                    break;
                default:
                    throw new Exception("CoordinateGARS.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }

        #endregion
    }

    public class CoordinateGARSFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateGARS)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    if (!string.IsNullOrEmpty(CoordinateBase.InputCustomFormat))
                    {
                        return this.Format(CoordinateBase.InputCustomFormat, arg, this);
                    }
                    return this.Format("X#YQK", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateGARS;
                    var cnum = coord.LonBand;
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
                            case 'X':
                                cnum = coord.LonBand;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'Y':
                                sb.Append(coord.LatBand);
                                break;
                            case 'Q':
                                sb.Append(coord.Quadrant.ToString());
                                break;
                            case 'K':
                                sb.Append(coord.Key.ToString());
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
