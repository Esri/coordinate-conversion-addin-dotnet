
/******************************************************************************* 
  * Copyright 2016 Esri 
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

namespace ProAppCoordConversionModule.ViewModels
{
    class ProSaveAsFormatViewModel : ProTabBaseViewModel
    {

        private bool featureIsChecked = true;
        public bool FeatureIsChecked
        {
            get
            {
                return featureIsChecked;
            }

            set
            {
                featureIsChecked = value;
                NotifyPropertyChanged(() => FeatureIsChecked);
            }
        }

        private bool shapeIsChecked = false;
        public bool ShapeIsChecked
        {
            get
            {
                return shapeIsChecked;
            }

            set
            {
                shapeIsChecked = value;
                NotifyPropertyChanged(() => ShapeIsChecked);
            }
        }

        private bool kmlIsChecked = false;
        public bool KmlIsChecked
        {
            get
            {
                return kmlIsChecked;
            }

            set
            {
                kmlIsChecked = value;
                NotifyPropertyChanged(() => KmlIsChecked);
            }
        }

        private bool csvIsChecked = false;
        public bool CSVIsChecked
        {
            get
            {
                return csvIsChecked;
            }

            set
            {
                csvIsChecked = value;
                NotifyPropertyChanged(() => CSVIsChecked);
            }
        }
    }  
}
