using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Models;
using System.Text.RegularExpressions;

namespace CoordinateToolLibrary
{
    public class CoordinateMGRS : CoordinateBase
    {
        public CoordinateMGRS() { }

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

            input = input.Trim();

            Regex regexMGRS = new Regex(@"^\s*(?<gzd>\d{1,2}[A-HJ-NP-Z])[ ]*(?<gs>[A-HJ-NP-Z]{2})[ ]*(?<eplusn>\d{2,18})\s*");

            var matchMGRS = regexMGRS.Match(input);

            if(matchMGRS.Success && matchMGRS.Length == input.Length)
            {
                if(ValidateNumericCoordinateMatch(matchMGRS, new string[] {"eplusn"}))
                {
                    // need to validate the gzd and gs
                    try
                    {
                        mgrs.GZD = matchMGRS.Groups["gzd"].Value;
                        mgrs.GS = matchMGRS.Groups["gs"].Value;
                        var tempEN = matchMGRS.Groups["eplusn"].Value;
                        if (tempEN.Length % 2 == 0)
                        {
                            int numSize = tempEN.Length / 2;
                            mgrs.Easting = Int32.Parse(tempEN.Substring(0, numSize));
                            mgrs.Northing = Int32.Parse(tempEN.Substring(numSize, numSize));
                        }
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
    }
}
