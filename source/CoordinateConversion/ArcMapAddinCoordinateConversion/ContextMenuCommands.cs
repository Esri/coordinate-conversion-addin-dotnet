/******************************************************************************* 
  * Copyright 2016 Esri 
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
using System.Linq;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Desktop.AddIns;
using CoordinateConversionLibrary.Models;

namespace ArcMapAddinCoordinateConversion
{
    public class ContextCopyBase : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ContextCopyBase()
        {
        }

        internal CoordinateType ctype = CoordinateType.Unknown;

        protected override void OnClick()
        {
            if (ArcMap.Document == null || ArcMap.Document.CurrentLocation == null 
                || ctype == CoordinateConversionLibrary.Models.CoordinateType.Unknown)
                return;

            var point = ArcMap.Document.CurrentLocation;
            if (point == null)
                return;

            string coord = string.Empty;
            try
            {
                var cn = (IConversionNotation)point;

                switch(ctype)
                {
                    case CoordinateType.DD:
                        coord = cn.GetDDFromCoords(6);
                        break;
                    case CoordinateType.DDM:
                        coord = cn.GetDDMFromCoords(6);
                        break;
                    case CoordinateType.DMS:
                        coord = cn.GetDMSFromCoords(2);
                        break;
                    case CoordinateType.GARS:
                        coord = cn.GetGARSFromCoords();
                        break;
                    case CoordinateType.MGRS:
                        coord = cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
                        break;
                    case CoordinateType.USNG:
                        coord = cn.GetUSNGFromCoords(5, true, true);
                        break;
                    case CoordinateType.UTM:
                        coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces | esriUTMConversionOptionsEnum.esriUTMUseNS);
                        break;
                    default:
                        break;
                }

                coord = CoordinateHandler.GetFormattedCoordinate(coord, ctype);
                
                System.Windows.Clipboard.SetText(coord);
            }
            catch { /* Conversion Failed */ }
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Document != null;
        }
    }

    public class ContextCopyDD : ContextCopyBase
    {
        public ContextCopyDD()
        {
            ctype = CoordinateType.DD;
        }
    }

    public class ContextCopyDDM : ContextCopyBase
    {
        public ContextCopyDDM()
        {
            ctype = CoordinateType.DDM;
        }
    }

    public class ContextCopyDMS : ContextCopyBase
    {
        public ContextCopyDMS()
        {
            ctype = CoordinateType.DMS;
        }
    }
    public class ContextCopyGARS : ContextCopyBase
    {
        public ContextCopyGARS()
        {
            ctype = CoordinateType.GARS;
        }
    }
    public class ContextCopyMGRS : ContextCopyBase
    {
        public ContextCopyMGRS()
        {
            ctype = CoordinateType.MGRS;
        }
    }
    public class ContextCopyUSNG : ContextCopyBase
    {
        public ContextCopyUSNG()
        {
            ctype = CoordinateType.USNG;
        }
    }
    public class ContextCopyUTM : ContextCopyBase
    {
        public ContextCopyUTM()
        {
            ctype = CoordinateType.UTM;
        }
    }

}
