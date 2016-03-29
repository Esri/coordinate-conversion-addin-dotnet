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


namespace CoordinateConversionLibrary.Models
{
    public class CoordinateUSNG : CoordinateMGRS
    {
        public CoordinateUSNG() : base()
        {
            GZD = "17T";
            GS = "QE";
            Easting = 16777;
            Northing = 44511;
        }

        public CoordinateUSNG(string gzd, string gsquare, int easting, int northing)
        {
            GZD = gzd;
            GS = gsquare;
            Northing = northing;
            Easting = easting;
        }

        public static bool TryParse(string input, out CoordinateUSNG usng)
        {
            CoordinateMGRS mgrs = new CoordinateMGRS();
            usng = new CoordinateUSNG(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);

            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                usng = new CoordinateUSNG(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
                return true;
            }
            usng = null;
            return false;
        }

    }
}
