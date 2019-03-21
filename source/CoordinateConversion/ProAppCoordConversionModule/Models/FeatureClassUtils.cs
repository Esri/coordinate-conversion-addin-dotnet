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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Esri
using ArcGIS.Desktop.Catalog;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;

using CoordinateConversionLibrary;
using System.Windows;
using System.Linq;

namespace ProAppCoordConversionModule.Models
{
    class FeatureClassUtils
    {
        private string previousLocation = "";

        /// <summary>
        /// Prompts the user to save features
        /// 
        /// </summary>
        /// <returns>The path to selected output (fgdb/shapefile)</returns>
        public string PromptUserWithSaveDialog(bool featureChecked, bool shapeChecked, bool kmlChecked, bool csvChecked)
        {
            //Prep the dialog
            SaveItemDialog saveItemDlg = new SaveItemDialog();
            saveItemDlg.Title = CoordinateConversionLibrary.Properties.Resources.TitleSelectOutput;
            saveItemDlg.OverwritePrompt = true;
            if (!string.IsNullOrEmpty(previousLocation))
                saveItemDlg.InitialLocation = previousLocation;


            // Set the filters and default extension
            if (featureChecked)
            {
                saveItemDlg.Filter = ItemFilters.geodatabaseItems_all;
            }
            else if (shapeChecked)
            {
                saveItemDlg.Filter = ItemFilters.shapefiles;
                saveItemDlg.DefaultExt = "shp";
            }
            else if (kmlChecked)
            {
                saveItemDlg.Filter = ItemFilters.kml;
                saveItemDlg.DefaultExt = "kmz";
            }
            else if (csvChecked)
            {
                saveItemDlg.Filter = "";
                saveItemDlg.DefaultExt = "csv";
            }

            bool? ok = saveItemDlg.ShowDialog();

            //Show the dialog and get the response
            if (ok == true)
            {
                string folderName = System.IO.Path.GetDirectoryName(saveItemDlg.FilePath);
                previousLocation = folderName;

                return saveItemDlg.FilePath;
            }
            return null;
        }

        /// <summary>
        /// Creates the output featureclass, either fgdb featureclass or shapefile
        /// </summary>
        /// <param name="outputPath">location of featureclass</param>
        /// <param name="saveAsType">Type of output selected, either fgdb featureclass or shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Output featureclass</returns>
        public async Task CreateFCOutput(string outputPath, SaveAsType saveAsType, List<ProGraphic> graphicsList, SpatialReference spatialRef, MapView mapview, GeomType geomType, bool isKML = false)
        {
            string dataset = System.IO.Path.GetFileName(outputPath);
            string connection = System.IO.Path.GetDirectoryName(outputPath);

            try
            {
                await QueuedTask.Run(async () =>
                {
                    await CreateFeatureClass(dataset, geomType, connection, spatialRef, graphicsList, mapview, isKML);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public async Task CreateFCOutput(string outputPath, SaveAsType saveAsType, List<CCProGraphic> mapPointList, SpatialReference spatialRef, MapView mapview, GeomType geomType, bool isKML = false)
        {
            string dataset = System.IO.Path.GetFileName(outputPath);
            string connection = System.IO.Path.GetDirectoryName(outputPath);

            try
            {
                await QueuedTask.Run(async () =>
                {
                    await CreateFeatureClass(dataset, connection, spatialRef, mapPointList, mapview, isKML);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Create polyline features from graphics and add to table
        /// </summary>
        /// <param name="graphicsList">List of graphics to add to table</param>
        /// <returns></returns>
        private static async Task CreateFeatures(List<ProGraphic> graphicsList)
        {
            ArcGIS.Core.Data.RowBuffer rowBuffer = null;

            try
            {
                await QueuedTask.Run(() =>
                {
                    var layer = MapView.Active.GetSelectedLayers()[0];
                    if (layer is FeatureLayer)
                    {
                        var featureLayer = (FeatureLayer)layer;

                        using (var table = featureLayer.GetTable())
                        {
                            TableDefinition definition = table.GetDefinition();
                            int shapeIndex = definition.FindField("Shape");

                            foreach (ProGraphic graphic in graphicsList)
                            {
                                rowBuffer = table.CreateRowBuffer();

                                if (graphic.Geometry is Polyline)
                                {
                                    Polyline poly = new PolylineBuilder(graphic.Geometry as Polyline).ToGeometry();
                                    rowBuffer[shapeIndex] = poly;
                                }
                                else if (graphic.Geometry is Polygon)
                                    rowBuffer[shapeIndex] = new PolygonBuilder(graphic.Geometry as Polygon).ToGeometry();

                                ArcGIS.Core.Data.Row row = table.CreateRow(rowBuffer);
                            }
                        }

                        //Get simple renderer from feature layer 
                        CIMSimpleRenderer currentRenderer = featureLayer.GetRenderer() as CIMSimpleRenderer;
                        if (currentRenderer != null)
                        {
                            CIMSymbolReference sybmol = currentRenderer.Symbol;

                            var outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 1.0, SimpleLineStyle.Solid);
                            var s = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.RedRGB, SimpleFillStyle.Null, outline);
                            CIMSymbolReference symbolRef = new CIMSymbolReference() { Symbol = s };
                            currentRenderer.Symbol = symbolRef;

                            featureLayer.SetRenderer(currentRenderer);
                        }
                    }
                });

            }
            catch (GeodatabaseException exObj)
            {
                System.Diagnostics.Debug.WriteLine(exObj);
                throw;
            }
            finally
            {
                if (rowBuffer != null)
                    rowBuffer.Dispose();
            }
        }
        private static async Task CreateFeatures(List<CCProGraphic> mapPointList)
        {
            ArcGIS.Core.Data.RowBuffer rowBuffer = null;

            try
            {
                await QueuedTask.Run(() =>
                {
                    var layer = MapView.Active.GetSelectedLayers()[0];
                    if (layer is FeatureLayer)
                    {
                        var featureLayer = (FeatureLayer)layer;

                        using (var table = featureLayer.GetTable())
                        {
                            TableDefinition definition = table.GetDefinition();
                            int shapeIndex = definition.FindField("Shape");

                            foreach (var point in mapPointList)
                            {
                                rowBuffer = table.CreateRowBuffer();

                                var geom = !point.MapPoint.HasZ ?
                                    new MapPointBuilder(point.MapPoint).ToGeometry() :
                                    MapPointBuilder.CreateMapPoint(point.MapPoint.X, point.MapPoint.Y, point.MapPoint.SpatialReference);
                                rowBuffer[shapeIndex] = geom;
                                foreach (var item in point.Attributes)
                                {
                                    int idx = definition.FindField(item.Key);
                                    if (idx > -1)
                                        rowBuffer[idx] = item.Value;
                                }
                                ArcGIS.Core.Data.Row row = table.CreateRow(rowBuffer);
                            }
                        }

                        //Get simple renderer from feature layer 
                        CIMSimpleRenderer currentRenderer = featureLayer.GetRenderer() as CIMSimpleRenderer;
                        if (currentRenderer != null)
                        {
                            CIMSymbolReference sybmol = currentRenderer.Symbol;

                            //var outline = SymbolFactory.ConstructStroke(ColorFactory.RedRGB, 1.0, SimpleLineStyle.Solid);
                            //var s = SymbolFactory.ConstructPolygonSymbol(ColorFactory.RedRGB, SimpleFillStyle.Null, outline);
                            var s = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 3.0);
                            CIMSymbolReference symbolRef = new CIMSymbolReference() { Symbol = s };
                            currentRenderer.Symbol = symbolRef;

                            featureLayer.SetRenderer(currentRenderer);
                        }
                    }
                });

            }
            catch (GeodatabaseException exObj)
            {
                System.Diagnostics.Debug.WriteLine(exObj);
                throw;
            }
            finally
            {
                if (rowBuffer != null)
                    rowBuffer.Dispose();
            }
        }

        private static IReadOnlyList<string> makeValueArray(string featureClass, string fieldName, string fieldType)
        {
            List<object> arguments = new List<object>();
            arguments.Add(featureClass);
            arguments.Add(fieldName);
            arguments.Add(fieldType);
            return Geoprocessing.MakeValueArray(arguments.ToArray());
        }

        /// <summary>
        /// Create a feature class
        /// </summary>
        /// <param name="dataset">Name of the feature class to be created.</param>
        /// <param name="featureclassType">Type of feature class to be created. Options are:
        /// <list type="bullet">
        /// <item>POINT</item>
        /// <item>MULTIPOINT</item>
        /// <item>POLYLINE</item>
        /// <item>POLYGON</item></list></param>
        /// <param name="connection">connection path</param>
        /// <param name="spatialRef">SpatialReference</param>
        /// <param name="graphicsList">List of graphics</param>
        /// <param name="mapview">MapView object</param>
        /// <param name="isKML">Is this a kml output</param>
        /// <returns></returns>
        private static async Task CreateFeatureClass(string dataset, GeomType geomType, string connection, SpatialReference spatialRef, List<ProGraphic> graphicsList, MapView mapview, bool isKML = false)
        {
            try
            {
                string strGeomType = geomType == GeomType.PolyLine ? "POLYLINE" : "POLYGON";

                List<object> arguments = new List<object>();
                // store the results in the geodatabase
                arguments.Add(connection);
                // name of the feature class
                arguments.Add(dataset);
                // type of geometry
                arguments.Add(strGeomType);
                // no template
                arguments.Add("");
                // no z values
                arguments.Add("DISABLED");
                // no m values
                arguments.Add("DISABLED");
                arguments.Add(spatialRef);

                var valueArray = Geoprocessing.MakeValueArray(arguments.ToArray());
                IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", valueArray);

                await CreateFeatures(graphicsList);

                if (isKML)
                {
                    await KMLUtils.ConvertLayerToKML(connection, dataset, MapView.Active);

                    // Delete temporary Shapefile
                    string[] extensionNames = { ".cpg", ".dbf", ".prj", ".shx", ".shp" };
                    string datasetNoExtension = Path.GetFileNameWithoutExtension(dataset);
                    foreach (string extension in extensionNames)
                    {
                        string shapeFile = Path.Combine(connection, datasetNoExtension + extension);
                        File.Delete(shapeFile);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private static async Task CreateFeatureClass(string dataset, string connection, SpatialReference spatialRef, List<CCProGraphic> mapPointList, MapView mapview, bool isKML = false)
        {
            try
            {
                List<object> arguments = new List<object>();
                // store the results in the geodatabase
                arguments.Add(connection);
                // name of the feature class
                arguments.Add(dataset);
                // type of geometry
                arguments.Add("POINT");
                // no template
                arguments.Add("");
                // m values
                arguments.Add("DISABLED");
                // no z values
                arguments.Add("DISABLED");
                arguments.Add(spatialRef);

                var env = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

                var valueArray = Geoprocessing.MakeValueArray(arguments.ToArray());

                IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management",
                    valueArray,
                    env,
                    null,
                    null,
                    GPExecuteToolFlags.Default);

                // Add additional fields based on type of graphic
                string nameNoExtension = Path.GetFileNameWithoutExtension(dataset);
                string featureClass = "";
                if (isKML)
                {
                    featureClass = connection + "/" + nameNoExtension + ".shp";
                }
                else
                {
                    featureClass = connection + "/" + dataset;
                }

                if (mapPointList.Count > 0 && mapPointList[0].Attributes != null && mapPointList[0].Attributes.Count > 0)
                {
                    foreach (var field in mapPointList[0].Attributes)
                    {
                        var lstDT = new List<DataType>();
                        foreach (var item in mapPointList.SelectMany(x => x.Attributes.Where(y => y.Key == field.Key).Select(y => y.Value)))
                        {
                            lstDT.Add(ParseString(item));
                        }
                        var dataType = "TEXT";
                        var totalCount = lstDT.Count;
                        var matchedCount = lstDT.Where(x => x == lstDT.FirstOrDefault()).Count();
                        if (totalCount == matchedCount)
                        {
                            if (lstDT.FirstOrDefault() == DataType.System_Boolean)
                                dataType = "TEXT";
                            else if (lstDT.FirstOrDefault() == DataType.System_DateTime)
                                dataType = "DATE";
                            else if (lstDT.FirstOrDefault() == DataType.System_Double)
                                dataType = "DOUBLE";
                            else if (lstDT.FirstOrDefault() == DataType.System_Int32)
                                dataType = "LONG";
                            else if (lstDT.FirstOrDefault() == DataType.System_Int64)
                                dataType = "DOUBLE";
                            else if (lstDT.FirstOrDefault() == DataType.System_String)
                                dataType = "TEXT";
                        }
                        else
                            dataType = "TEXT";
                        IGPResult addFieldResult = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, field.Key, dataType));
                    }
                }

                await CreateFeatures(mapPointList);

                if (isKML)
                {
                    await KMLUtils.ConvertLayerToKML(connection, dataset, MapView.Active);

                    // Delete temporary Shapefile
                    string[] extensionNames = { ".cpg", ".dbf", ".prj", ".shx", ".shp" };
                    string datasetNoExtension = Path.GetFileNameWithoutExtension(dataset);
                    foreach (string extension in extensionNames)
                    {
                        string shapeFile = Path.Combine(connection, datasetNoExtension + extension);
                        File.Delete(shapeFile);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static DataType ParseString(string str)
        {

            bool boolValue;
            Int32 intValue;
            Int64 bigintValue;
            double doubleValue;
            DateTime dateValue;

            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(str, out boolValue))
                return DataType.System_Boolean;
            else if (Int32.TryParse(str, out intValue))
                return DataType.System_Int32;
            else if (Int64.TryParse(str, out bigintValue))
                return DataType.System_Int64;
            else if (double.TryParse(str, out doubleValue))
                return DataType.System_Double;
            else if (DateTime.TryParse(str, out dateValue))
                return DataType.System_DateTime;
            else return DataType.System_String;

        }

        public static async Task<List<Dictionary<string, Tuple<object,bool>>>> ImportFromExcel(string fileName)
        {
            var tableName = "ExcelData";
            var lstDictionary = new List<Dictionary<string, Tuple<object,bool>>>();
            List<object> arguments = new List<object>();
            arguments.Add(fileName);
            arguments.Add(tableName);
            var env = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            var valueArray = Geoprocessing.MakeValueArray(arguments.ToArray());
            var progressDialog = new ProgressDialog("Processing.. Please wait");
            progressDialog.Show();
            try
            {
                IGPResult result = await Geoprocessing.ExecuteToolAsync("ExcelToTable_conversion", valueArray, env, null, null, GPExecuteToolFlags.Default);
                if (!result.IsFailed)
                {
                    lstDictionary = await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                    {
                        using (Geodatabase geodatabase = new Geodatabase(FgdbFileToConnectionPath(CoreModule.CurrentProject.DefaultGeodatabasePath)))
                        {
                            QueryDef queryDef = new QueryDef
                            {
                                Tables = tableName,
                                WhereClause = "1 = 1",
                            };

                            using (RowCursor rowCursor = geodatabase.Evaluate(queryDef, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    var dictionary = new Dictionary<string, Tuple<object,bool>>();
                                    using (ArcGIS.Core.Data.Row row = rowCursor.Current)
                                    {
                                        for (int i = 0; i < row.GetFields().Count; i++)
                                        {
                                            var key = row.GetFields()[i].Name;
                                            var val = rowCursor.Current[i];
                                            dictionary.Add(key, Tuple.Create(val,false));
                                        }
                                    }
                                    lstDictionary.Add(dictionary);
                                }
                            }
                        }
                        progressDialog.Hide();
                        return lstDictionary;
                    });
                }
                else
                {
                    progressDialog.Hide();
                    MessageBox.Show("ExcelToTable_conversion operation failed.");
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                progressDialog.Hide();
            }
            return lstDictionary;
        }

        public static FileGeodatabaseConnectionPath FgdbFileToConnectionPath(string fGdbPath)
        {
            return new FileGeodatabaseConnectionPath(new Uri(CoreModule.CurrentProject.DefaultGeodatabasePath));
        }
    }

    enum DataType
    {
        System_Boolean = 0,
        System_Int32 = 1,
        System_Int64 = 2,
        System_Double = 3,
        System_DateTime = 4,
        System_String = 5
    }
}
