using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinCoordinateTool
{
    public class ArcMapCoordinateGet : CoordinateToolLibrary.Models.CoordinateGetBase
    {
        public ArcMapCoordinateGet()
        { }

        public IPoint Point { get; set; }

        #region Can Gets

        public override bool CanGetDD(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDDFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDDM(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDDMFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDMS(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDMSFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetGARS(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetGARSFromCoords();
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetMGRS(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    // 5 numeric units in MGRS is 1m resolution
                    var cn = Point as IConversionNotation;
                    coord = cn.CreateMGRS(5, false, esriMGRSModeEnum.esriMGRSMode_NewStyle);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUSNG(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetUSNGFromCoords(5, false, false);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUTM(out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    var cn = Point as IConversionNotation;
                    coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS);
                    return true;
                }
                catch { }
            }
            return false;
        }

        #endregion

        //#region Getters
        //public override string GetDD()
        //{
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            return cn.GetDDFromCoords(6);
        //        }
        //        catch { }
        //    }
        //    return string.Empty;
        //}

        //public override string GetDDM()
        //{
        //    return string.Empty;
        //}

        //public override string GetDMS()
        //{
        //    return string.Empty;
        //}

        //public override string GetGARS()
        //{
        //    return string.Empty;
        //}

        //public override string GetMGRS()
        //{
        //    return string.Empty;
        //}

        //public override string GetUSNG()
        //{
        //    return string.Empty;
        //}

        //public override string GetUTM()
        //{
        //    return string.Empty;
        //}
        //#endregion
    }
}
