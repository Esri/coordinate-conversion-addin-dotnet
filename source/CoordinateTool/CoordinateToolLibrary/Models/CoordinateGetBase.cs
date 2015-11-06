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
        public virtual bool CanGetDD(int srFactoryCode, out string coord)
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

        public virtual bool CanGetDDM(int srFactoryCode, out string coord)
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

        public virtual bool CanGetDMS(int srFactoryCode, out string coord)
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

        public virtual bool CanGetGARS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetMGRS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUSNG(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUTM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        #endregion

        // support project and spatial reference

        // configure
        public virtual bool SelectSpatialReference()
        {
            return false;
        }

        public virtual void Project(int factoryCode)
        {
            // do nothing
        }

    }
}
