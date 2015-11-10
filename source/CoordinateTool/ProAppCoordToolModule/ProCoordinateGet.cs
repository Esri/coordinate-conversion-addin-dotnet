using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateToolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordToolModule
{
    public class ProCoordinateGet : CoordinateToolLibrary.Models.CoordinateGetBase
    {
        public ProCoordinateGet()
        { }

        public MapPoint Point { get; set; }

        #region Can Gets

        //public override bool CanGetDD(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetDDFromCoords(6);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        public override bool CanGetDDM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if(base.CanGetDDM(srFactoryCode, out coord))
            {
                return true;
            }
            else
            {
                if(base.CanGetDD(srFactoryCode, out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if(CoordinateDD.TryParse(coord, out dd))
                    {
                        var ddm = new CoordinateDDM(dd);
                        coord = ddm.ToString("", new CoordinateDDMFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool CanGetDMS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (base.CanGetDMS(srFactoryCode, out coord))
            {
                return true;
            }
            else
            {
                if (base.CanGetDD(srFactoryCode, out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if (CoordinateDD.TryParse(coord, out dd))
                    {
                        var dms = new CoordinateDMS(dd);
                        coord = dms.ToString("", new CoordinateDMSFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        //public override bool CanGetGARS(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetGARSFromCoords();
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetMGRS(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            // 5 numeric units in MGRS is 1m resolution
        //            var cn = Point as IConversionNotation;
        //            coord = cn.CreateMGRS(5, false, esriMGRSModeEnum.esriMGRSMode_NewStyle);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetUSNG(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetUSNGFromCoords(5, false, false);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetUTM(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        public override bool SelectSpatialReference()
        {
            return false;
        }

        public override void Project(int factoryCode)
        {
            var temp = QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.SpatialReference spatialReference = SpatialReferenceBuilder.CreateSpatialReference(factoryCode);

                Point = (MapPoint)GeometryEngine.Project(Point, spatialReference);

                return true;
            }).Result;
        }
        #endregion
    }
}
