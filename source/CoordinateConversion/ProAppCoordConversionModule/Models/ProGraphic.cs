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

using System;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;

namespace ProAppCoordConversionModule.Models
{
    public class ProGraphic
    {
        public ProGraphic(IDisposable _disposable, string guid, Geometry _geometry, CIMSymbolReference symbol, bool _isTemp = false, string tag = "")
        {
            Disposable = _disposable;
            GUID = guid;
            Geometry = _geometry;
            IsTemp = _isTemp;
            Tag = tag;
            SymbolRef = symbol;
        }

        // properties   

        /// <summary>
        /// Property to hold the disposable object
        /// calling dispose on this object will remove it from the map
        /// </summary>
        //public string UniqueId { get; set; }
        public IDisposable Disposable { get; set; }

        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp { get; set; }

        /// <summary>
        /// optional tag property
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// CIMSymbolReference
        /// </summary>
        public CIMSymbolReference SymbolRef { get; set; }
    }
}
