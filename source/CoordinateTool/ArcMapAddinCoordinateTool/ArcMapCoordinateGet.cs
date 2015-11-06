using CoordinateToolLibrary.Helpers;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        public override bool CanGetDD(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDDFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDDM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDDMFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetDMS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetDMSFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetGARS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetGARSFromCoords();
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetMGRS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    // 5 numeric units in MGRS is 1m resolution
                    var cn = Point as IConversionNotation;
                    coord = cn.CreateMGRS(5, false, esriMGRSModeEnum.esriMGRSMode_NewStyle);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUSNG(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetUSNGFromCoords(5, false, false);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public override bool CanGetUTM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = Point as IConversionNotation;
                    coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS);
                    return true;
                }
                catch { }
            }
            return false;
        }

        #endregion

        public override bool SelectSpatialReference()
        {
            // get dialog handle
            ESRI.ArcGIS.CatalogUI.ISpatialReferenceDialog2 spatialReferenceDialog = new ESRI.ArcGIS.CatalogUI.SpatialReferenceDialogClass();
            ESRI.ArcGIS.Geometry.ISpatialReference spatialReference = spatialReferenceDialog.DoModalCreate(true, false, false, (int)System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

            if (spatialReference != null)
            {
                Mediator.NotifyColleagues("SRSELECTED", string.Format("{0}::{1}", spatialReference.FactoryCode, spatialReference.Name));
                return true;
            }

            return false;
        }

        public override void Project(int srfactoryCode)
        {
            ISpatialReference sr = null;

            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            try
            {
                var geographicCS = srFact.CreateGeographicCoordinateSystem(srfactoryCode);

                sr = geographicCS as ISpatialReference;
            }
            catch { }

            if(sr == null)
            {
                try
                {
                    var projectedCS = srFact.CreateProjectedCoordinateSystem(srfactoryCode);

                    sr = projectedCS as ISpatialReference;
                }
                catch { }
            }
            
            if (sr == null)
                return;

            try
            {
                Point.Project(sr);
            }
            catch { }
        }
    }
}
