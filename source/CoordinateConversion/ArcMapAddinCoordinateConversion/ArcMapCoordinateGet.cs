/******************************************************************************* 
  * Copyright 2015 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/ 

using System;
using ESRI.ArcGIS.Geometry;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Models;

namespace ArcMapAddinCoordinateConversion
{
    public class ArcMapCoordinateGet : CoordinateConversionLibrary.Models.CoordinateGetBase
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
                    var cn = (IConversionNotation)Point;
                    coord = cn.GetDMSFromCoords(6);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public bool CanGetGARS(out string coord)
        {
            return CanGetGARS(4326, out coord);
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

        public bool CanGetMGRS(out string coord)
        {
            return CanGetMGRS(4326, out coord);
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
                    coord = cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public bool CanGetUSNG(out string coord)
        {
            return CanGetUSNG(4326, out coord);
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
                    coord = cn.GetUSNGFromCoords(5, true, false);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public bool CanGetUTM(out string coord)
        {
            return CanGetUTM(4326, out coord);
        }

        public override bool CanGetUTM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            if (Point != null)
            {
                try
                {
                    Project(srFactoryCode);
                    var cn = (IConversionNotation)Point;
                    coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces);
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
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SpatialReferenceSelected, string.Format("{0}::{1}", spatialReference.FactoryCode, spatialReference.Name));
                return true;
            }

            return false;
        }

        public override void Project(int srfactoryCode)
        {
            if (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType == CoordinateTypes.None)
                return;

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

        public string GetInputDisplayString()
        {
            string result = "NA";
            try
            {
                if (Point == null || Point.IsEmpty)
                    return result;

                result = string.Format("{0:0.0#####} {1:0.0#####}", Point.Y, Point.X);

                if (Point.SpatialReference == null)
                    return result;

                var cn = Point as IConversionNotation;
                if (cn != null)
                {
                    switch (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType)
                    {
                        case CoordinateTypes.DD:
                            result = cn.GetDDFromCoords(6);
                            break;
                        case CoordinateTypes.DDM:
                            result = cn.GetDDMFromCoords(4);
                            break;
                        case CoordinateTypes.DMS:
                            result = cn.GetDMSFromCoords(2);
                            break;
                        //case CoordinateTypes.GARS:
                        //    result = cn.GetGARSFromCoords();
                        //    break;
                        case CoordinateTypes.MGRS:
                            result = cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
                            break;
                        case CoordinateTypes.USNG:
                            result = cn.GetUSNGFromCoords(5, true, false);
                            break;
                        case CoordinateTypes.UTM:
                            result = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch 
            {
                return "NA";
            }
            return result;
        }
    }
}
