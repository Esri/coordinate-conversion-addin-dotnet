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
    public class CoordinateUTM : CoordinateBase
    {
        public CoordinateUTM()
        {
            Zone = 17;
            Band = "S";
            Easting = 716777;
            Northing = 4444511;
        }

        public CoordinateUTM(int zone, string band, int easting, int northing)
        {
            Zone = zone;
            Band = band;
            Easting = easting;
            Northing = northing;
        }

        public int Zone { get; set; }
        public string Band { get; set; }
        public string Hemi
        {
            get
            {
                if (Convert.ToChar(this.Band) >= 'N')
                    return "N";
                else
                    return "S";
            }
        }
        public int Easting { get; set; }
        public int Northing { get; set; }

        #region Methods

        public static bool TryParse(string input, out CoordinateUTM utm)
        {
            utm = new CoordinateUTM();

            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            input = input.Trim();

            Regex regexUTM = new Regex(@"^\s*(?<zone>\d{1,2})(?<band>[A-HJ-NP-Z]?)[-,;:\sm]*(?<easting>\d{1,9})[-,;:\sm]*(?<northing>\d{1,9})[-,;:\sm]*");

            var matchUTM = regexUTM.Match(input);

            if (matchUTM.Success && matchUTM.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchUTM, new string[] { "zone","easting","northing" }))
                {
                    // need to validate the gzd and gs
                    try
                    {
                        utm.Zone = Int32.Parse(matchUTM.Groups["zone"].Value);
                        utm.Easting = Int32.Parse(matchUTM.Groups["easting"].Value);
                        utm.Northing = Int32.Parse(matchUTM.Groups["northing"].Value);
                        utm.Band = matchUTM.Groups["band"].Value;
                    }
                    catch
                    {
                        /* Conversion Failed */
                        return false;
                    }

                    return Validate(utm);
                }
            }

            return false;
        }

        public static bool Validate(CoordinateUTM utm)
        {
            if (utm.Zone < 1 || utm.Zone > 60)
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
                format = "UTM";

            NumberFormatInfo fi = NumberFormatInfo.InvariantInfo;

            switch (format.ToUpper())
            {
                case "":
                case "UTM":
                    sb.Append(Zone);
                    sb.Append(Hemi+" ");
                    sb.AppendFormat(fi, "{0:#}", Easting);
                    sb.AppendFormat(fi, "{0:#}", Northing);
                    break;
                default:
                    throw new Exception("CoordinateUTM.ToString(): Invalid formatting string.");
            }

            return sb.ToString();
        }

        #endregion
    }

    public class CoordinateUTMFormatter : CoordinateFormatterBase
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is CoordinateUTM)
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    if (!string.IsNullOrEmpty(CoordinateBase.InputCustomFormat))
                    {
                        return this.Format(CoordinateBase.InputCustomFormat, arg, this);
                    }
                    return this.Format("Z#B X0 Y0", arg, this);
                }
                else
                {
                    var coord = arg as CoordinateUTM;
                    var cnum = coord.Zone;
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
                            case 'Z': 
                                cnum = coord.Zone;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
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
                            case '+': // show +
                                if (coord.Hemi == "N")
                                    sb.Append("+");
                                break;
                            case '-': // show -
                                if (coord.Hemi == "S")
                                    sb.Append("-");
                                break;
                            case 'H': // N or S
                                sb.Append(coord.Hemi);
                                break;
                            case 'B': // Latitude Band
                                sb.Append(coord.Band);
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
