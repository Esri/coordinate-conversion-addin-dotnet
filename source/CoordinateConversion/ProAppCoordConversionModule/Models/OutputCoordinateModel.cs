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
using System.Collections.Generic;
using System.Windows;
using ProAppCoordConversionModule.Common;
using System.Xml.Serialization;

namespace ProAppCoordConversionModule.Models
{
    public class OutputCoordinateModel : NotificationObject
    {
        public OutputCoordinateModel()
        {
            DVisibility = Visibility.Collapsed;
            _props = new Dictionary<string, string>();
        }

        #region Details Visibility
        [XmlIgnore]
        public Visibility DVisibility { get; set; }
        #endregion

        #region Name

        private string name = String.Empty;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        #region Props
        private Dictionary<string, string> _props;
        [XmlIgnore]
        public Dictionary<string, string> Props 
        {
            get
            {
                return _props;
            }
            set
            {
                _props = value;
                RaisePropertyChanged(() => Props);
            }
        }
        #endregion

        #region OutputCoordinate

        private string outputCoordinate = String.Empty;

        [XmlIgnore]
        public string OutputCoordinate
        {
            get { return outputCoordinate; }
            set
            {
                outputCoordinate = value;
                RaisePropertyChanged(() => OutputCoordinate);
            }
        }

        #endregion

        #region CType
        public CoordinateType CType { get; set; }
        #endregion

        #region Format
        private string format = "Y-+#0.0000 X-+##0.0000";
        public string Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                RaisePropertyChanged(() => Format);
            }
        }
        #endregion Format

        private int srFactoryCode = 4326;
        public int SRFactoryCode
        {
            get
            {
                return srFactoryCode;
            }
            set
            {
                if (srFactoryCode != value)
                {
                    srFactoryCode = value;
                    RaisePropertyChanged(() => SRFactoryCode);
                }
            }
        }

        private string srName = "WGS84";
        public string SRName
        {
            get
            {
                return srName;
            }

            set
            {
                if(srName != value)
                {
                    srName = value;
                    RaisePropertyChanged(() => SRName);
                }
            }
        }

        #region Methods
        public void ToggleVisibility()
        {
            if (this.DVisibility == Visibility.Collapsed)
                this.DVisibility = Visibility.Visible;
            else
                this.DVisibility = Visibility.Collapsed;

            RaisePropertyChanged(() => DVisibility);
        }
        #endregion
    }
}
