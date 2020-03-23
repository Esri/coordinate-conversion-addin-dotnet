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


namespace ProAppCoordConversionModule.Helpers
{
    public class Constants
    {
        public const int MAX_HISTORY_COUNT = 5;

        //public const string CopyAllCoordinateOutputs = "COPY_ALL_COORDINATE_OUTPUTS";
        //public const string SelectSpatialReference = "SELECTSR";
        //public const string AddNewOutputCoordinate = "ADD_NEW_OUTPUT_COORDINATE";
        //public const string NewMapPointSelection = "NEW_MAP_POINT_SELECTION";
                                        

        public const string DefaultCustomFormat = "Y0.0##### X0.0#####";
        public const string DDCustomFormat = "Y0.0##### X0.0#####";
        public const string DDMCustomFormat = "A0° B0.0###' X0° Y0.0###'";
        public const string DMSCustomFormat = "A0° B0' C0.0#\" X0° Y0' Z0.0#\"";
        public const string MGRSCustomFormat = "ZSX00000Y00000";
        public const string USNGCustomFormat = "ZSX00000Y00000";
        public const string UTMCustomFormat = "Z#B X0 Y0";
        public const double SymbolSize = 10;
        public const string LayerToKMLGPTool = "LayerToKML_conversion";
    }
}
