/******************************************************************************* 
  * Copyright 2018 Esri 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateConversionLibrary.Models;

namespace CoordinateConversionLibrary.Helpers
{
    public class ConversionUtils
    {
        /// <summary>
        /// Returns a formatted coordinate string for an input coordinate string
        /// IMPORTANT: if a coordinate format is not matched: 
        /// Returns CoordinateType.Unknown, the input string as the formattedString 
        /// </summary>
        /// <param name="input">Input coord string</param>
        /// <param name="formattedString">Formatted coord string</param>
        /// <returns>CoordinateType of the format that matched, Unknown if unmatched</returns>
        public static CoordinateType GetCoordinateString(string input, out string formattedString)
        {
            // We don't want the Ambiguous Coords Dialog to show during these calls
            // But don't overwrite the setting in case user has set to prompt for this
            bool savedAmbigCoordSetting = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;

            formattedString = input;
            // DD
            CoordinateDD dd;
            if (CoordinateDD.TryParse(input, out dd) == true)
            {
                formattedString = dd.ToString("Y0.0#N X0.0#E", new CoordinateDDFormatter());
                return CoordinateType.DD;
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm) == true)
            {
                dd = new CoordinateDD(ddm);
                formattedString = ddm.ToString("", new CoordinateDDMFormatter());
                return CoordinateType.DDM;
            }

            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms) == true)
            {
                dd = new CoordinateDD(dms);
                formattedString = dms.ToString("A0°B0'C0.0##\"N X0°Y0'Z0.0##\"E", new CoordinateDMSFormatter());
                return CoordinateType.DMS;
            }

            // restore the setting (not needed for others, only DD/DMS
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = savedAmbigCoordSetting;

            // GARS
            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars) == true)
            {
                formattedString = gars.ToString("", new CoordinateGARSFormatter());
                return CoordinateType.GARS;
            }

            // MGRS
            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs) == true)
            {
                formattedString = mgrs.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
                return CoordinateType.MGRS;
            }

            // USNG
            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng) == true)
            {
                formattedString = usng.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
                return CoordinateType.USNG;
            }

            // UTM
            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm) == true)
            {
                formattedString = utm.ToString("", new CoordinateUTMFormatter());
                return CoordinateType.UTM;
            }

            return CoordinateType.Unknown;
        }
    }
}
