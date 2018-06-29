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

// Esri
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ADF;
using CoordinateConversionLibrary;

namespace ArcMapAddinCoordinateConversion.Models
{
    class FeatureClassUtils
    {
        private IGxDialog m_ipSaveAsGxDialog = null;

        /// <summary>
        /// Prompts the user to save features
        /// 
        /// Use "this.Handle.ToInt32()" as the parentWindow id 
        /// </summary>
        /// <param name="iParentWindow">The window handle of the parent window</param>
        /// <returns>The path to selected output (fgdb/shapefile)</returns>
        public string PromptUserWithGxDialog(int iParentWindow)
        {
            //Prep the dialog
            if (m_ipSaveAsGxDialog == null)
            {
                m_ipSaveAsGxDialog = new GxDialog();
                IGxObjectFilterCollection ipGxObjFilterCol = (IGxObjectFilterCollection)m_ipSaveAsGxDialog;
                ipGxObjFilterCol.RemoveAllFilters();

                // Add the filters
                ipGxObjFilterCol.AddFilter(new GxFilterFGDBFeatureClasses(), false);
                ipGxObjFilterCol.AddFilter(new GxFilterShapefilesClass(), false);

                m_ipSaveAsGxDialog.AllowMultiSelect = false;
                m_ipSaveAsGxDialog.Title = CoordinateConversionLibrary.Properties.Resources.TitleSelectOutput;
                m_ipSaveAsGxDialog.ButtonCaption = CoordinateConversionLibrary.Properties.Resources.ButtonOK;
                m_ipSaveAsGxDialog.RememberLocation = true;
            }
            else
            {
                m_ipSaveAsGxDialog.Name = "";
                m_ipSaveAsGxDialog.FinalLocation.Refresh();
            }

            //Show the dialog and get the response
            if (m_ipSaveAsGxDialog.DoModalSave(iParentWindow) == false)
                return null;
            else
            {
                IGxObject ipGxObject = m_ipSaveAsGxDialog.FinalLocation;
                string nameString = m_ipSaveAsGxDialog.Name;
                bool replacingObject = m_ipSaveAsGxDialog.ReplacingObject;
                string path = m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                IGxObject ipSelectedObject = m_ipSaveAsGxDialog.InternalCatalog.SelectedObject;

                // user selected an existing featureclass
                if (ipSelectedObject != null && ipSelectedObject is IGxDataset)
                {
                    IGxDataset ipGxDataset = (IGxDataset)ipSelectedObject;
                    IDataset ipDataset = ipGxDataset.Dataset;

                    // User will be prompted if they select an existing shapefile
                    if (ipDataset.Category.Equals("Shapefile Feature Class"))
                    {
                        return path;
                    }

                    while (DoesFeatureClassExist(ipDataset.Workspace.PathName, m_ipSaveAsGxDialog.Name))
                    {
                        if (System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.MsgOverwrite,
                                                                 CoordinateConversionLibrary.Properties.Resources.CaptionOverwrite,
                                                                 System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                        {
                            return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                        }

                        if (m_ipSaveAsGxDialog.DoModalSave(iParentWindow) == false)
                        {
                            return null;
                        }

                        ipGxDataset = (IGxDataset)ipSelectedObject;
                        ipDataset = ipGxDataset.Dataset;
                    }

                    return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                }
                else
                    return path;
            }
        }

        /// <summary>
        /// Creates the output featureclass, either fgdb featureclass or a shapefile
        /// </summary>
        /// <param name="outputPath">location of featureclass</param>
        /// <param name="saveAsType">Type of output selected, either fgdb featureclass or shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Output featureclass</returns>
        public IFeatureClass CreateFCOutput(string outputPath, SaveAsType saveAsType, List<CCAMGraphic> graphicsList, ISpatialReference ipSpatialRef)
        {
            string fcName = System.IO.Path.GetFileName(outputPath);
            string folderName = System.IO.Path.GetDirectoryName(outputPath);
            IFeatureClass fc = null;

            try
            {
                //bool isGraphicLineOrRangeRing = graphicsList[0].GraphicType == GraphicTypes.Line || graphicsList[0].GraphicType == GraphicTypes.RangeRing;
                if (saveAsType == SaveAsType.FileGDB)
                {
                    IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
                    IWorkspace workspace = workspaceFactory.OpenFromFile(folderName, 0);
                    IFeatureWorkspace fWorkspace = (IFeatureWorkspace)workspace;

                    if (DoesFeatureClassExist(folderName, fcName))
                    {
                        DeleteFeatureClass(fWorkspace, fcName);
                    }


                    if (graphicsList.Count > 0 && graphicsList[0].Attributes != null && graphicsList[0].Attributes.Keys != null)
                    {
                        var fieldKeys = graphicsList[0].Attributes.Keys;

                        fc = CreateFeatureClass(fWorkspace, fieldKeys, fcName);

                        foreach (var graphic in graphicsList)
                        {
                            IFeature feature = fc.CreateFeature();

                            feature.Shape = graphic.MapPoint.Geometry;
                            foreach (var item in graphic.Attributes)
                            {
                                int idx = feature.Fields.FindField(item.Key);
                                if (idx > -1)
                                    feature.set_Value(idx, item.Value);
                            }

                            feature.Store();
                        }
                    }
                }
                else if (saveAsType == SaveAsType.Shapefile)
                {
                    // already asked them for confirmation to overwrite file
                    if (File.Exists(outputPath))
                    {
                        DeleteShapeFile(outputPath);
                    }

                    fc = ExportToShapefile(outputPath, graphicsList, ipSpatialRef);
                }
                return fc;
            }
            catch (Exception ex)
            {
                return fc;
            }

            return fc;
        }

        public void DeleteShapeFile(string shapeFilePath)
        {
            string fcName = System.IO.Path.GetFileName(shapeFilePath);
            string folderName = System.IO.Path.GetDirectoryName(shapeFilePath);

            using (ComReleaser oComReleaser = new ComReleaser())
            {
                IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
                IWorkspace workspace = workspaceFactory.OpenFromFile(folderName, 0);
                IFeatureWorkspace fWorkspace = (IFeatureWorkspace)workspace;
                IDataset ipDs = fWorkspace.OpenFeatureClass(fcName) as IDataset;
                ipDs.Delete();

                File.Delete(shapeFilePath);

                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workspace);
                workspace = null;
                fWorkspace = null;
                ipDs = null;
            }

            GC.Collect();

        }

        /// <summary>
        /// Export graphics to a shapefile
        /// </summary>
        /// <param name="fileNamePath">Path to shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Created featureclass</returns>
        private IFeatureClass ExportToShapefile(string fileNamePath, List<CCAMGraphic> graphicsList, ISpatialReference ipSpatialRef)
        {
            int index = fileNamePath.LastIndexOf('\\');
            string folder = fileNamePath.Substring(0, index);
            string nameOfShapeFile = fileNamePath.Substring(index + 1);
            string shapeFieldName = "Shape";
            IFeatureClass featClass = null;

            using (ComReleaser oComReleaser = new ComReleaser())
            {
                try
                {
                    IWorkspaceFactory workspaceFactory = null;
                    workspaceFactory = new ShapefileWorkspaceFactoryClass();
                    IWorkspace workspace = workspaceFactory.OpenFromFile(folder, 0);
                    IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
                    IFields fields = null;
                    IFieldsEdit fieldsEdit = null;
                    fields = new Fields();
                    fieldsEdit = (IFieldsEdit)fields;
                    IField field = null;
                    IFieldEdit fieldEdit = null;
                    field = new FieldClass();///###########
                    fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = "Shape";
                    fieldEdit.Type_2 = (esriFieldType.esriFieldTypeGeometry);
                    IGeometryDef geomDef = null;
                    IGeometryDefEdit geomDefEdit = null;
                    geomDef = new GeometryDefClass();///#########
                    geomDefEdit = (IGeometryDefEdit)geomDef;

                    geomDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;

                    geomDefEdit.SpatialReference_2 = ipSpatialRef;

                    fieldEdit.GeometryDef_2 = geomDef;
                    fieldsEdit.AddField(field);

                    if (graphicsList.Count > 0)
                    {
                        if (graphicsList[0].Attributes != null && graphicsList[0].Attributes.Count > 0)
                        {
                            foreach (var fieldName in graphicsList[0].Attributes.Keys)
                            {
                                field = new FieldClass();
                                fieldEdit = (IFieldEdit)field;
                                fieldEdit.Name_2 = fieldName;
                                fieldEdit.AliasName_2 = fieldName;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldsEdit.AddField(field);
                            }
                        }
                    }

                    featClass = featureWorkspace.CreateFeatureClass(nameOfShapeFile, fields, null, null, esriFeatureType.esriFTSimple, shapeFieldName, "");

                    foreach (var graphic in graphicsList)
                    {
                        IFeature feature = featClass.CreateFeature();

                        feature.Shape = graphic.MapPoint.Geometry;
                        foreach (var item in graphic.Attributes)
                        {
                            int idx = feature.Fields.FindField(item.Key);
                            if (idx > -1)
                                feature.set_Value(idx, item.Value);
                        }
                        feature.Store();
                    }

                    IFeatureLayer featurelayer = null;
                    featurelayer = new FeatureLayerClass();
                    featurelayer.FeatureClass = featClass;
                    featurelayer.Name = featClass.AliasName;

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workspace);
                    workspace = null;
                    GC.Collect();

                    return featClass;
                }
                catch (Exception ex)
                {
                    return featClass;
                }
            }
        }

        /// <summary>
        /// Determines if selected feature class already exists
        /// </summary>
        /// <param name="gdbPath">Path to the file gdb</param>
        /// <param name="fcName">Name of selected feature class</param>
        /// <returns>True if already exists, false otherwise</returns>
        private bool DoesFeatureClassExist(string gdbPath, string fcName)
        {
            List<string> dsNames = GetAllDatasetNames(gdbPath);

            if (dsNames.Contains(fcName))
                return true;

            return false;
        }

        /// <summary>
        /// Retrieves all datasets names from filegdb
        /// </summary>
        /// <param name="gdbFilePath">Path to filegdb</param>
        /// <returns>List of names of all featureclasses in filegdb</returns>
        private List<string> GetAllDatasetNames(string gdbFilePath)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile(gdbFilePath, 0);
            IEnumDataset enumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
            List<string> names = new List<string>();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                names.Add(dataset.Name);
            }
            return names;
        }

        /// <summary>
        /// Delete a featureclass
        /// </summary>
        /// <param name="fWorkspace">IFeatureWorkspace</param>
        /// <param name="fcName">Name of featureclass to delete</param>
        private void DeleteFeatureClass(IFeatureWorkspace fWorkspace, string fcName)
        {
            IDataset ipDs = fWorkspace.OpenFeatureClass(fcName) as IDataset;
            ipDs.Delete();
        }

        /// <summary> 
        /// Create the polyline feature class 
        /// </summary> 
        /// <param name="featWorkspace">IFeatureWorkspace</param> 
        /// <param name="name">Name of the featureclass</param> 
        /// <returns>IFeatureClass</returns> 
        private IFeatureClass CreateFeatureClass(IFeatureWorkspace featWorkspace, Dictionary<string, string>.KeyCollection fieldNames, string name)
        {
            IFieldsEdit pFldsEdt = new FieldsClass();
            IFieldEdit pFldEdt = new FieldClass();

            pFldEdt.Type_2 = esriFieldType.esriFieldTypeOID;
            pFldEdt.Name_2 = "OBJECTID";
            pFldEdt.AliasName_2 = "OBJECTID";
            pFldsEdt.AddField(pFldEdt);

            IGeometryDefEdit pGeoDef;
            pGeoDef = new GeometryDefClass();
            pGeoDef.GeometryType_2 = esriGeometryType.esriGeometryPoint;

            pGeoDef.SpatialReference_2 = ArcMap.Document.FocusMap.SpatialReference;

            pFldEdt = new FieldClass();
            pFldEdt.Name_2 = "SHAPE";
            pFldEdt.AliasName_2 = "SHAPE";
            pFldEdt.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFldEdt.GeometryDef_2 = pGeoDef;
            pFldsEdt.AddField(pFldEdt);
            if (fieldNames != null && fieldNames.Count > 0)
            {
                foreach (var field in fieldNames)
                {
                    pFldEdt = new FieldClass();
                    pFldEdt.Name_2 = field;
                    pFldEdt.AliasName_2 = field;
                    pFldEdt.Type_2 = esriFieldType.esriFieldTypeString;
                    pFldsEdt.AddField(pFldEdt);
                }
            }

            IFeatureClass pFClass = featWorkspace.CreateFeatureClass(name, pFldsEdt, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            return pFClass;
        }

        /// <summary>
        /// Convert a polyline feature to a polygon
        /// </summary>
        /// <param name="geom">IGeometry</param>
        /// <returns>IPolygon</returns>
        private IPolygon PolylineToPolygon(IGeometry geom)
        {
            //Build a polygon segment-by-segment.
            IPolygon polygon = new PolygonClass();
            Polyline polyLine = geom as Polyline;

            ISegmentCollection polygonSegs = polygon as ISegmentCollection;
            ISegmentCollection polylineSegs = polyLine as ISegmentCollection;

            for (int i = 0; i < polylineSegs.SegmentCount; i++)
            {
                ISegment seg = polylineSegs.Segment[i];
                polygonSegs.AddSegment(seg);
            }

            polygon.SimplifyPreserveFromTo();

            return polygon;

        }
    }
}
