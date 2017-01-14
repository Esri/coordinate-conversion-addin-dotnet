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
using System.Linq;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateHandler
    {
        public CoordinateHandler()
        { }

        public static string GetFormattedCoord(CoordinateType cType, string coord, string format)
        {
            if (cType == CoordinateType.DD)
            {
                CoordinateDD dd;
                if (CoordinateDD.TryParse(coord, out dd))
                {
                    return dd.ToString(format, new CoordinateDDFormatter());
                }
            }
            if (cType == CoordinateType.DDM)
            {
                CoordinateDDM ddm;
                if (CoordinateDDM.TryParse(coord, out ddm))
                {
                    return ddm.ToString(format, new CoordinateDDMFormatter());
                }
            }
            if (cType == CoordinateType.DMS)
            {
                CoordinateDMS dms;
                if (CoordinateDMS.TryParse(coord, out dms))
                {
                    return dms.ToString(format, new CoordinateDMSFormatter());
                }
            }
            /*if (cType == CoordinateType.GARS)
            {
                CoordinateGARS gars;
                if (CoordinateGARS.TryParse(coord, out gars))
                {
                    return gars.ToString(format, new CoordinateGARSFormatter());
                }
            }*/
            if (cType == CoordinateType.MGRS)
            {
                CoordinateMGRS mgrs;
                if (CoordinateMGRS.TryParse(coord, out mgrs))
                {
                    return mgrs.ToString(format, new CoordinateMGRSFormatter());
                }
            }
            if (cType == CoordinateType.USNG)
            {
                CoordinateUSNG usng;
                if (CoordinateUSNG.TryParse(coord, out usng))
                {
                    return usng.ToString(format, new CoordinateMGRSFormatter());
                }
            }
            if (cType == CoordinateType.UTM)
            {
                CoordinateUTM utm;
                if (CoordinateUTM.TryParse(coord, out utm))
                {
                    return utm.ToString(format, new CoordinateUTMFormatter());
                }
            }

            return null;
        }

        public static string GetFormattedCoordinate(string coord, CoordinateType cType)
        {
            string format = "";

            var outputCoordinate = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.FirstOrDefault(t => t.CType == cType);
            if (outputCoordinate != null)
            {
                format = outputCoordinate.Format;
                //Console.WriteLine(tt.Format);
            }

            var formattedCoordinate = CoordinateHandler.GetFormattedCoord(cType, coord, format);

            if (!String.IsNullOrWhiteSpace(formattedCoordinate))
                return formattedCoordinate;

            return string.Empty;
        }

    }
}
