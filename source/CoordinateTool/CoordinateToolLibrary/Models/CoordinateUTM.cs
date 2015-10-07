using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public class CoordinateUTM : CoordinateBase
    {
        public CoordinateUTM()
        {
            Zone = 17;
            Hemi = "N";
            Easting = 1000;
            Northing = 1000;
        }

        public CoordinateUTM(int zone, string hemi, int easting, int northing)
        {
            Zone = zone;
            Hemi = hemi;
            Easting = easting;
            Northing = northing;
        }

        public int Zone { get; set; }
        public string Hemi { get; set; } 
        public int Easting { get; set; }
        public int Northing { get; set; }

        #region Methods

        public static bool TryParse(string input, out CoordinateUTM utm)
        {
            utm = new CoordinateUTM();

            input = input.Trim();

            Regex regexUTM = new Regex(@"^\s*(?<zone>\d{1,2})(?<hemi>[NS]?)\s*(?<easting>\d{1,9})\s*(?<northing>\d{1,9})\s*");

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
                        utm.Hemi = matchUTM.Groups["hemi"].Value;
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
                    return this.Format("Z#H E#m N#m", arg, this);
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

                    foreach (char c in format.ToUpper())
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
                            case 'Z': 
                                cnum = coord.Zone;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'E': // easting
                                cnum = coord.Easting;
                                olist.Add(Math.Abs(cnum));
                                startIndexNeeded = true;
                                break;
                            case 'N': // northing
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
                            case 'M': 
                                sb.Append("m");
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
