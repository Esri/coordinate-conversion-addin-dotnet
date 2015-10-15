using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
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

            input = input.Trim();

            Regex regexGARS = new Regex(@"^\s*(?<lonband>\d{3})[-,;:\s]*(?<latband>[A-HJ-NP-Z]{2}?)[-,;:\s]*(?<quadrant>\d?)[-,;:\s]*(?<key>\d?)\s*");

            var matchGARS = regexGARS.Match(input);

            if (matchGARS.Success && matchGARS.Length == input.Length)
            {
                if (ValidateNumericCoordinateMatch(matchGARS, new string[] { "lonband", "quadrant", "key" }))
                {
                    // need to validate the latband
                    try
                    {
                        gars.LonBand = Int32.Parse(matchGARS.Groups["lonband"].Value);
                        gars.LatBand = matchGARS.Groups["latband"].Value;
                        gars.Quadrant = Int32.Parse(matchGARS.Groups["quadrant"].Value);
                        gars.Key = Int32.Parse(matchGARS.Groups["key"].Value);
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
