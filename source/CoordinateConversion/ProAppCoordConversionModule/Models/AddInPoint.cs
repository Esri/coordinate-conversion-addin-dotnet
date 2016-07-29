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

using ArcGIS.Core.Geometry;
using CoordinateConversionLibrary.Helpers;
using ProAppCoordConversionModule.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordConversionModule.Models
{
    public class AddInPoint : NotificationObject
    {
        public AddInPoint()
        {

        }

        private MapPoint point = null;
        public MapPoint Point
        {
            get
            {
                return point;
            }
            set
            {
                point = value;

                RaisePropertyChanged(() => Point);
                RaisePropertyChanged(() => Text);
            }
        }
        public string Text
        {
            get { return MapPointHelper.GetMapPointAsDisplayString(Point); }
        }

        private string guid = string.Empty;
        public string GUID
        {
            get
            {
                return guid;
            }
            set
            {
                guid = value;
                RaisePropertyChanged(() => GUID);
            }
        }
        /// <summary>
        /// Property used to determine if it is selected in the listbox
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
