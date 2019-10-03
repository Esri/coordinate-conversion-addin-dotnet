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
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;

namespace ArcMapAddinCoordinateConversion.Models
{
    class KMLUtils
    {

        public bool ConvertLayerToKML(IFeatureClass fc, string kmzOutputPath, string tmpShapefilePath, ESRI.ArcGIS.Carto.IMap map)
        {
            try
            {
                string kmzName = System.IO.Path.GetFileName(kmzOutputPath);
                string folderName = System.IO.Path.GetDirectoryName(kmzOutputPath);
                var fcName = System.IO.Path.GetFileNameWithoutExtension(kmzName);

                IFeatureLayer fLayer = new FeatureLayer();
                fLayer.FeatureClass = fc;
                var geoLayer = (fLayer as IGeoFeatureLayer);
                if (geoLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
                    pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                    pSimpleMarkerSymbol.Size = CoordinateConversionLibrary.Constants.SymbolSize;
                    pSimpleMarkerSymbol.Color = new RgbColorClass() { Red = 255 };
                    ISimpleRenderer pSimpleRenderer;
                    pSimpleRenderer = new SimpleRenderer();
                    pSimpleRenderer.Symbol = (ISymbol)pSimpleMarkerSymbol;
                    geoLayer.Name = fcName;
                    geoLayer.Renderer = (IFeatureRenderer)pSimpleRenderer;
                }
                var featureLayer = geoLayer as FeatureLayer;

                map.AddLayer(geoLayer);

                // Initialize the geoprocessor.
                IGeoProcessor2 gp = new GeoProcessorClass();
                IVariantArray parameters = new VarArrayClass();
                parameters.Add(featureLayer.Name);
                parameters.Add(folderName + "\\" + kmzName);
                var result = gp.Execute(CoordinateConversionLibrary.Constants.LayerToKMLGPTool, parameters, null);

                // Remove the temporary layer from the TOC
                for (int i = 0; i < map.LayerCount; i++)
                {
                    ILayer layer = map.get_Layer(i);
                    if (layer.Name == fcName)
                    {
                        map.DeleteLayer(layer);
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return false;
        }
    }


}
