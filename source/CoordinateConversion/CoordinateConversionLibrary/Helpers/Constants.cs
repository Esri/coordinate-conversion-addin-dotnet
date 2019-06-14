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
        public const string ClearOutputCoordinates = "BROADCAST_CLEAR_OUTPUT_COORDINATES";
        public const string AddNewOutputCoordinate = "ADD_NEW_OUTPUT_COORDINATE";
        public const string BroadcastCoordinateValues = "BROADCAST_COORDINATE_VALUES";
        public const string RequestOutputUpdate = "REQUEST_OUTPUT_UPDATE";
        public const string SetListBoxItemAddInPoint = "SET_LISTBOX_ITEM_ADDINPOINT";
        public const string NewMapPointSelection = "NEW_MAP_POINT_SELECTION";
        public const string SetCoordinateGetter = "SET_COORDINATE_GETTER";
        public const string NEW_MAP_POINT = "NEW_MAP_POINT";
        public const string MOUSE_MOVE_POINT = "MOUSE_MOVE_POINT";
        public const string IMPORT_COORDINATES = "IMPORT_COORDINATES";
        public const string CollectListHasItems = "COLLECT_LIST_HAS_ITEMS";
        public const string SELECT_MAP_POINT = "SELECT_MAP_POINT";
        public const string DEACTIVATE_TOOL = "DEACTIVATE_TOOL";
        public const string VALIDATE_MAP_POINT = "VALIDATE_MAP_POINT";

        public const string DefaultCustomFormat = "Y0.0##### X0.0#####";
        public const string DDCustomFormat = "Y0.0##### X0.0#####";
        public const string DDMCustomFormat = "A0° B0.0###' X0° Y0.0###'";
        public const string DMSCustomFormat = "A0° B0' C0.0#\" X0° Y0' Z0.0#\"";
        public const string MGRSCustomFormat = "ZSX00000Y00000";
        public const string USNGCustomFormat = "ZSX00000Y00000";
        public const string UTMCustomFormat = "Z#B X0 Y0";

    }
}
