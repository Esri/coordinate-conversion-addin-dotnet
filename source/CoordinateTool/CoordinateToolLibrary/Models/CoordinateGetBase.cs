using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public class CoordinateGetBase
    {
        public CoordinateGetBase()
        {

        }

        public string InputCoordinate { get; set; }

        #region Can Gets
        public virtual bool CanGetDD(out string coord)
        {
            CoordinateDD dd;
            if (CoordinateDD.TryParse(InputCoordinate, out dd))
            {
                coord = dd.ToString("", new CoordinateDDFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetDDM(out string coord)
        {
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(InputCoordinate, out ddm))
            {
                coord = ddm.ToString("", new CoordinateDDMFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetDMS(out string coord)
        {
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(InputCoordinate, out dms))
            {
                coord = dms.ToString("", new CoordinateDMSFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetGARS(out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetMGRS(out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUSNG(out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUTM(out string coord)
        {
            coord = string.Empty;
            return false;
        }

        #endregion

    }
}
