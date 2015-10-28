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
            GZD = "17T";
            GS = "QE";
            Easting = 16777;
            Northing = 44511;
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
            CoordinateMGRS mgrs = new CoordinateMGRS();
            usng = new CoordinateUSNG(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);

            if (string.IsNullOrWhiteSpace(input))
                return false;

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
