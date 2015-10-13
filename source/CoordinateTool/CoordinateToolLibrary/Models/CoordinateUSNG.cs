using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public class CoordinateUSNG : CoordinateMGRS
    {
        public CoordinateUSNG() : base()
        {
        }

        public CoordinateUSNG(string gzd, string gsquare, int easting, int northing)
        {
            GZD = gzd;
            GS = gsquare;
            Northing = northing;
            Easting = easting;
        }

        public static bool TryParse(string input, out CoordinateUSNG usng)
        {
            CoordinateMGRS mgrs;

            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                usng = new CoordinateUSNG(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
                return true;
            }
            usng = null;
            return false;
        }

    }
}
