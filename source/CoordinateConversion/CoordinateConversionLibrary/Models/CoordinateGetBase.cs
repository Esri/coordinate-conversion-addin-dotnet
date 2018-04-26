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
    public class CoordinateGetBase
    {
        public CoordinateGetBase()
        {

        }

        public string InputCoordinate { get; set; }

        #region Can Gets
        public virtual bool CanGetDD(int srFactoryCode, out string coord)
        {
            CoordinateDD dd;
            if (CoordinateDD.TryParse(InputCoordinate, true, out dd))
            {
                Project(srFactoryCode);
                coord = dd.ToString("", new CoordinateDDFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetDDM(int srFactoryCode, out string coord)
        {
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(InputCoordinate, true, out ddm))
            {
                coord = ddm.ToString("", new CoordinateDDMFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetDMS(int srFactoryCode, out string coord)
        {
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(InputCoordinate, true, out dms))
            {
                coord = dms.ToString("", new CoordinateDMSFormatter());
                return true;
            }
            else
            {
                coord = string.Empty;
                return false;
            }
        }

        public virtual bool CanGetGARS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetMGRS(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUSNG(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        public virtual bool CanGetUTM(int srFactoryCode, out string coord)
        {
            coord = string.Empty;
            return false;
        }

        #endregion

        // support project and spatial reference

        // configure
        public virtual bool SelectSpatialReference()
        {
            return false;
        }

        public virtual void Project(int factoryCode)
        {
            // do nothing
        }

    }
}
