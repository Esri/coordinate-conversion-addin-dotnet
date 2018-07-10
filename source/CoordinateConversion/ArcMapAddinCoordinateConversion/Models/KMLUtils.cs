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

// System
using System;

// Esri
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;

namespace ArcMapAddinCoordinateConversion.Models
{
    class KMLUtils
    {
        
        public bool ConvertLayerToKML(string kmzOutputPath, string tmpShapefilePath, ESRI.ArcGIS.Carto.IMap map)
        {
            try
            {
                string kmzName = System.IO.Path.GetFileName(kmzOutputPath);
                string folderName = System.IO.Path.GetDirectoryName(kmzOutputPath);

                IGeoProcessor2 gp = new GeoProcessorClass();
                IVariantArray parameters = new VarArrayClass();
                parameters.Add(tmpShapefilePath);
                parameters.Add(kmzName);
                gp.Execute("MakeFeatureLayer_management", parameters, null);

                IVariantArray parameters1 = new VarArrayClass();
                // assign  parameters        
                parameters1.Add(kmzName);
                parameters1.Add(kmzOutputPath);

                gp.Execute("LayerToKML_conversion", parameters1, null);

                // Remove the temporary layer from the TOC
                for (int i = 0; i < map.LayerCount; i++ )
                {
                    ILayer layer = map.get_Layer(i);
                    if (layer.Name == "featureLayer")
                    {
                        map.DeleteLayer(layer);
                        break;
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return false;
        }
    }

    
}
