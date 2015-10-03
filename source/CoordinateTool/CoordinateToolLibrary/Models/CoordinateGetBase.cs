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
            coord = string.Empty;
            return true;
        }

        public virtual bool CanGetDDM(out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetDMS(out string coord)
        {
            coord = string.Empty;
            return true;
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

        //#region Getters
        //public virtual string GetDD()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetDDM()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetDMS()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetGARS()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetMGRS()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetUSNG()
        //{
        //    return string.Empty;
        //}

        //public virtual string GetUTM()
        //{
        //    return string.Empty;
        //}
        //#endregion
    }
}
