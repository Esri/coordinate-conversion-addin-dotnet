// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CoordinateConversionLibrary.Properties;

namespace CoordinateConversionLibrary
{

    public enum GeomType : int
    {
        Point = 1,
        PolyLine = 2,
        Polygon = 3
    }

    public enum SaveAsType : int
    {
        FileGDB = 1,
        Shapefile = 2,
        KML = 3
    }

    public enum CoordinateTypes : int
    {
        [LocalizableDescription(@"EnumCTDD", typeof(Resources))]
        DD = 1,

        [LocalizableDescription(@"EnumCTDDM", typeof(Resources))]
        DDM = 2,

        [LocalizableDescription(@"EnumCTDMS", typeof(Resources))]
        DMS = 3,

        //[LocalizableDescription(@"EnumCTGARS", typeof(Resources))]
        //GARS = 4,

        [LocalizableDescription(@"EnumCTMGRS", typeof(Resources))]
        MGRS = 5,

        [LocalizableDescription(@"EnumCTUSNG", typeof(Resources))]
        USNG = 6,

        [LocalizableDescription(@"EnumCTUTM", typeof(Resources))]
        UTM = 7,

        [LocalizableDescription(@"EnumCTNone", typeof(Resources))]
        None = 8
    }

    /// <summary>
    /// Enumeration used for the different tool modes
    /// </summary>
    public enum MapPointToolMode : int
    {
        Unknown = 0,
        Convert = 1,
        Collect = 2
    }

}
