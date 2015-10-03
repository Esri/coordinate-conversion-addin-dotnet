using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public class CoordinateHandler
    {
        public CoordinateHandler()
        { }

        public bool Parse(string input, out CoordinateDD dd)
        {
            dd = new CoordinateDD();
            CoordinateDMS dms;
            
            // try to parse all types?

            if(CoordinateDD.TryParse(input, out dd))
            {
                return true;
            }
            else if(CoordinateDMS.TryParse(input, out dms))
            {
                dd.Lat = Math.Abs(dms.LatDegrees) + (dms.LatMinutes / 60.0) + (dms.LatSeconds / 3600.0);
                dd.Lon = Math.Abs(dms.LonDegrees) + (dms.LonMinutes / 60.0) + (dms.LonSeconds / 3600.0);

                if (dms.LatDegrees < 0)
                    dd.Lat *= -1;

                if (dms.LonDegrees < 0)
                    dd.Lon *= -1;

                return true;
            }

            return false;
        }

        public CoordinateDMS GetDMS(CoordinateDD dd)
        {
            var dms = new CoordinateDMS();
            var tlat = Math.Truncate(dd.Lat);
            var tlon = Math.Truncate(dd.Lon);
            var latminDec = (Math.Abs(dd.Lat) - Math.Abs(tlat)) * 60.0;
            var lonminDec = (Math.Abs(dd.Lon) - Math.Abs(tlon)) * 60.0;
            dms.LatDegrees = (int)tlat;
            dms.LatMinutes = (int)latminDec;
            dms.LatSeconds = (latminDec - Math.Truncate(latminDec)) * 60.0;
            dms.LonDegrees = (int)tlon;
            dms.LonMinutes = (int)lonminDec;
            dms.LonSeconds = (lonminDec - Math.Truncate(lonminDec)) * 60.0;

            return dms;
        }
    }
}
