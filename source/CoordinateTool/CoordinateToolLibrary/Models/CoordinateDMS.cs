using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
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
            double latm = (dd.Lat - Math.Truncate(dd.Lat)) * 60.0;
            LatMinutes = (int)Math.Truncate(latm);
            LatSeconds = (latm - LatMinutes) * 60.0;

            LonDegrees = (int)Math.Truncate(dd.Lon);
            double lonm = (dd.Lon - Math.Truncate(dd.Lon)) * 60.0;
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

            Regex regexDMS = new Regex("^ *[+]*(?<latitudeSuffix>[NS])?(?<latitudeD>[^NSDd*° ,:]*)?[Dd*° ,:]*(?<latitudeM>[^NS' ,:]*)?[' ,:]*(?<latitudeS>[^NS\\\" ,:]*)?[\\\" ,:]*(?<latitudeSuffix>[NS])? *[+,]*(?<longitudeSuffix>[EW])?(?<longitudeD>[^EWDd*° ,:]*)?[Dd*° ,:]*(?<longitudeM>[^EW' ,:]*)?[' ,:]*(?<longitudeS>[^EW\\\" ,:]*)?[\\\" ,:]*(?<longitudeSuffix>[EW])?[\\\"]*", RegexOptions.ExplicitCapture); 

            var matchDMS = regexDMS.Match(input);
            
            if (matchDMS.Success && matchDMS.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchDMS, new string[] { "latitudeD", "latitudeM", "latitudeS", "longitudeD", "longitudeM", "longitudeS" }))
                {
                    try
                    {
                        var LatDegrees = int.Parse(matchDMS.Groups["latitudeD"].Value);
                        var LatMinutes = int.Parse(matchDMS.Groups["latitudeM"].Value);
                        var LatSeconds = double.Parse(matchDMS.Groups["latitudeS"].Value);
                        var LonDegrees = int.Parse(matchDMS.Groups["longitudeD"].Value);
                        var LonMinutes = int.Parse(matchDMS.Groups["longitudeM"].Value);
                        var LonSeconds = double.Parse(matchDMS.Groups["longitudeS"].Value);

                        var temp = matchDMS.Groups["latitudeSuffix"];
                        if (temp.Success && temp.Value.ToUpper().Equals("S"))
                        {
                            LatDegrees = Math.Abs(LatDegrees) * -1;
                        }
                        temp = matchDMS.Groups["longitudeSuffix"];
                        if (temp.Success && temp.Value.ToUpper().Equals("W"))
                        {
                            LonDegrees = Math.Abs(LonDegrees) * -1;
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
                    return this.Format("A0°B0'C0.0\"N X0°Y0'Z0.0\"E", arg, this);
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
