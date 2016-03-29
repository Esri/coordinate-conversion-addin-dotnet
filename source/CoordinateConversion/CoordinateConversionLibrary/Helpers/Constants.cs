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


namespace CoordinateConversionLibrary
{
    public class Constants
    {
        public const int MAX_HISTORY_COUNT = 5;

        public const string CopyAllCoordinateOutputs = "COPY_ALL_COORDINATE_OUTPUTS";
        public const string SelectSpatialReference = "SELECTSR";
        public const string SpatialReferenceSelected = "SRSELECTED";
        public const string RequestCoordinateBroadcast = "BROADCAST_COORDINATE_NEEDED";
        public const string AddNewOutputCoordinate = "ADD_NEW_OUTPUT_COORDINATE";
        public const string BroadcastCoordinateValues = "BROADCAST_COORDINATE_VALUES";
        public const string RequestOutputUpdate = "REQUEST_OUTPUT_UPDATE";
        public const string DISPLAY_COORDINATE_TYPE_CHANGED = "DISPLAY_COORDINATE_TYPE_CHANGED";
        public const string ConfigLoaded = "ConfigLoaded";
    }
}
