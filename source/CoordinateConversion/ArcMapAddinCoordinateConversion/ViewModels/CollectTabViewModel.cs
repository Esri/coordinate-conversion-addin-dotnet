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

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using CoordinateConversionLibrary.Helpers;
using ArcMapAddinCoordinateConversion.Models;
using ArcMapAddinCoordinateConversion.Helpers;
using ArcMapAddinCoordinateConversion.Views;
using ESRI.ArcGIS.Geodatabase;
using CoordinateConversionLibrary;
using System.Windows.Forms;
using System.Text;
using CoordinateConversionLibrary.Models;
using Jitbit.Utils;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class CollectTabViewModel : ArcMapTabBaseViewModel
    {
        public CollectTabViewModel()
        {
            ListBoxItemAddInPoint = null;

            CoordinateAddInPoints = new ObservableCollection<AddInPoint>();

            DeletePointCommand = new RelayCommand(OnDeletePointCommand);
            DeleteAllPointsCommand = new RelayCommand(OnDeleteAllPointsCommand);
            ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);
            EnterKeyCommand = new RelayCommand(OnEnterKeyCommand);
            SaveAsCommand = new RelayCommand(OnSaveAsCommand);
            CopyCoordinateCommand = new RelayCommand(OnCopyCommand);
            CopyAllCoordinatesCommand = new RelayCommand(OnCopyAllCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
            Mediator.Register(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, OnImportCoordinates);
        }

        public bool HasListBoxRightClickSelectedItem
        {
            get
            {
                return ListBoxItemAddInPoint != null;
            }
        }
        public bool HasAnySelectedItems
        {
            get
            {
                return CoordinateAddInPoints.Any(p => p.IsSelected == true);
            }
        }

        public AddInPoint ListBoxItemAddInPoint { get; set; }

        public ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }

        private object _ListBoxSelectedItem = null;
        public object ListBoxSelectedItem
        {
            get { return _ListBoxSelectedItem; }
            set
            {
                // we are using this to know when the selection changes
                // setting null will allow this setter to be called in multiple selection mode
                _ListBoxSelectedItem = null;
                RaisePropertyChanged(() => ListBoxSelectedItem);

                // update selections
                UpdateHighlightedGraphics();
            }
        }

        private int _ListBoxSelectedIndex = -1;
        public int ListBoxSelectedIndex
        {
            get { return _ListBoxSelectedIndex; }
            set
            {
                // this works with the ListBoxSelectedItem 
                // without this the un-selecting of 1 will not trigger an update
                _ListBoxSelectedIndex = value;
                RaisePropertyChanged(() => ListBoxSelectedIndex);
                UpdateHighlightedGraphics();
            }
        }

        public RelayCommand DeletePointCommand { get; set; }
        public RelayCommand DeleteAllPointsCommand { get; set; }
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand EnterKeyCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand CopyCoordinateCommand { get; set; }
        public RelayCommand CopyAllCoordinatesCommand { get; set; }

        // lists to store GUIDs of graphics, temp feedback and map graphics
        private static List<AMGraphic> GraphicsList = new List<AMGraphic>();

        private void OnDeletePointCommand(object obj)
        {
            var items = obj as IList;
            var objects = items.Cast<AddInPoint>().ToList();

            if (objects == null)
                return;

            DeletePoints(objects);
        }

        private void OnDeleteAllPointsCommand(object obj)
        {
            DeletePoints(CoordinateAddInPoints.ToList());
        }

        private void OnClearGraphicsCommand(object obj)
        {
            /* KG - Use DeletePoints() to Clrea All button
            var mxdoc = ArcMap.Application.Document as IMxDocument;
            if (mxdoc == null)
                return;
            var av = mxdoc.FocusMap as IActiveView;
            if (av == null)
                return;
            var gc = av as IGraphicsContainer;
            if (gc == null)
                return;
            //TODO need to clarify what clear graphics button does
            // seems to be different than the other Military Tools when doing batch collection
            RemoveGraphics(gc, GraphicsList.Where(g => g.IsTemp == false).ToList());

            //av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            av.Refresh(); // sometimes a partial refresh is not working
            */
            DeletePoints(CoordinateAddInPoints.ToList());
        }

        // Call CopyAllCoordinateOutputs event
        private void OnCopyAllCommand(object obj)
        {
            OnCopyAllCoordinateOutputs(CoordinateAddInPoints.ToList());
        }

        // copy parameter to clipboard
        private void OnCopyCommand(object obj)
        {
            var items = obj as IList;
            var objects = items.Cast<AddInPoint>().ToList();

            if (objects == null)
                return;            

            if (objects == null || !objects.Any())
                return;

            var sb = new StringBuilder();
            foreach (var point in objects)
            {
                sb.AppendLine(point.Text);
            }

            if (sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }

            foreach (var point in objects)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(point.Text);
            }
        }

        private void OnEnterKeyCommand(object obj)
        {
            if(!HasInputError && InputCoordinate.Length > 0)
            {
                AddCollectionPoint(amCoordGetter.Point);
            }
        }

        private void OnSaveAsCommand(object obj)
        {
            var saveAsDialog = new AMSaveAsFormatView();
            var vm = new SaveAsFormatViewModel();
            saveAsDialog.DataContext = vm;

            if (saveAsDialog.ShowDialog() == true)
            {
                IFeatureClass fc = null;
                string path = null;
                var fcUtils = new FeatureClassUtils();
                if (vm.FeatureShapeIsChecked)
                {
                    path = fcUtils.PromptUserWithGxDialog(ArcMap.Application.hWnd);
                    if (path != null)
                    {
                        if (System.IO.Path.GetExtension(path).Equals(".shp"))
                        {
                            fc = fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, GraphicsList, ArcMap.Document.FocusMap.SpatialReference);
                        }
                        else
                        {
                            fc = fcUtils.CreateFCOutput(path, SaveAsType.FileGDB, GraphicsList, ArcMap.Document.FocusMap.SpatialReference);
                        }
                    }
                }
                else if (vm.KmlIsChecked)
                {
                    path = PromptSaveFileDialog("kmz", "KMZ File (*.kmz)|*.kmz", CoordinateConversionLibrary.Properties.Resources.KMLLocationMessage);
                    if (path != null)
                    {
                        string kmlName = System.IO.Path.GetFileName(path);
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        string tempShapeFile = folderName + "\\tmpShapefile.shp";
                        IFeatureClass tempFc = fcUtils.CreateFCOutput(tempShapeFile, SaveAsType.Shapefile, GraphicsList, ArcMap.Document.FocusMap.SpatialReference);

                        if (tempFc != null)
                        {
                            var kmlUtils = new KMLUtils();
                            kmlUtils.ConvertLayerToKML(path, tempShapeFile, ArcMap.Document.FocusMap);

                            // delete the temporary shapefile
                            fcUtils.DeleteShapeFile(tempShapeFile);
                        }
                    }
                }
                else
                {
                    //Export to CSV
                    path = PromptSaveFileDialog("csv", "CSV File (*.csv)|*.csv", CoordinateConversionLibrary.Properties.Resources.CSVLocationMessage);
                    if (path != null)
                    {                        
                        string csvName = System.IO.Path.GetFileName(path);
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        string tempFile = System.IO.Path.Combine(folderName, csvName);

                        var aiPoints = CoordinateAddInPoints.ToList();

                        if (aiPoints == null || !aiPoints.Any())
                            return;

                        var csvExport = new CsvExport();
                        foreach (var point in aiPoints)
                        {
                            csvExport.AddRow();
                            csvExport["Coordinates"] = point.Text;
                        }
                        csvExport.ExportToFile(tempFile);

                        System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.CSVExportSuccessfulMessage + tempFile,
                            CoordinateConversionLibrary.Properties.Resources.CSVExportSuccessfulCaption);                        
                    }
                }

                if (fc != null)
                {
                    AddFeatureLayerToMap(fc);
                }
            }
        }

        /// <summary>
        /// Copies all coordinates to the clipboard
        /// </summary>
        /// <param name="obj"></param>
        private void OnCopyAllCoordinateOutputs(List<AddInPoint> aiPoints)
        {
            var sb = new StringBuilder();

            if (aiPoints == null || !aiPoints.Any())
                return;

            foreach (var point in aiPoints)
            {
                sb.AppendLine(point.Text);
            }

            if (sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }

        /// <summary>
        /// Add the feature layer to the map 
        /// </summary>
        /// <param name="fc">IFeatureClass</param>
        private void AddFeatureLayerToMap(IFeatureClass fc)
        {
            IFeatureLayer outputFeatureLayer = new FeatureLayerClass();
            outputFeatureLayer.FeatureClass = fc;

            IGeoFeatureLayer geoLayer = outputFeatureLayer as IGeoFeatureLayer;
            if(geoLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                IFeatureRenderer pFeatureRender;
                pFeatureRender = (IFeatureRenderer)new SimpleRenderer();
                ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
                pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                pSimpleMarkerSymbol.Size = 3.0;

                ISimpleRenderer pSimpleRenderer;
                pSimpleRenderer = new SimpleRenderer();
                pSimpleRenderer.Symbol = (ISymbol)pSimpleMarkerSymbol;

                geoLayer.Renderer = (IFeatureRenderer)pSimpleRenderer;
            }
            else if (geoLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                IFeatureRenderer pFeatureRender;
                pFeatureRender = (IFeatureRenderer)new SimpleRenderer();
                ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
                pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSHollow;
                pSimpleFillSymbol.Outline.Width = 0.4;

                ISimpleRenderer pSimpleRenderer;
                pSimpleRenderer = new SimpleRenderer();
                pSimpleRenderer.Symbol = (ISymbol)pSimpleFillSymbol;

                geoLayer.Renderer = (IFeatureRenderer)pSimpleRenderer;
            }

            geoLayer.Name = fc.AliasName;

            ESRI.ArcGIS.Carto.IMap map = ArcMap.Document.FocusMap;
            map.AddLayer((ILayer)outputFeatureLayer);
        }

        private SaveFileDialog sfDlg = null;
        private string PromptSaveFileDialog(string ext, string filter, string title)
        {
            if (sfDlg == null)
            {
                sfDlg = new SaveFileDialog();
                sfDlg.AddExtension = true;
                sfDlg.CheckPathExists = true;
                sfDlg.DefaultExt = ext; 
                sfDlg.Filter = filter;
                sfDlg.OverwritePrompt = true;
                sfDlg.Title = title;

            }
            sfDlg.FileName = "";

            if (sfDlg.ShowDialog() == DialogResult.OK)
            {
                return sfDlg.FileName;
            }

            return null;
        }

        /// <summary>
        /// Method used to remove graphics from the graphics container
        /// Elements are tagged with a GUID on the IElementProperties.Name property
        /// </summary>
        /// <param name="gc">map graphics container</param>
        /// <param name="list">list of GUIDs to remove</param>
        private void RemoveGraphics(IGraphicsContainer gc, List<AMGraphic> list)
        {
            if (gc == null || !list.Any())
                return;

            var elementList = new List<IElement>();
            gc.Reset();
            var element = gc.Next();
            while (element != null)
            {
                var eleProps = element as IElementProperties;
                if (list.Any(g => g.UniqueId == eleProps.Name))
                {
                    elementList.Add(element);
                }
                element = gc.Next();
            }

            foreach (var ele in elementList)
            {
                gc.DeleteElement(ele);
            }

            // remove from master graphics list
            foreach (var graphic in list)
            {
                if (GraphicsList.Contains(graphic))
                    GraphicsList.Remove(graphic);
            }
            elementList.Clear();
        }

        private void UpdateHighlightedGraphics()
        {
            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var gc = av as IGraphicsContainer;

            gc.Reset();
            var element = gc.Next();

            while (element != null)
            {
                var eProp = element as IElementProperties;

                if (eProp != null)
                {
                    var aiPoint = CoordinateAddInPoints.FirstOrDefault(p => p.GUID == eProp.Name);

                    if (aiPoint != null)
                    {
                        // highlight
                        var markerElement = element as IMarkerElement;
                        if (markerElement != null)
                        {
                            var sms = markerElement.Symbol as ISimpleMarkerSymbol;
                            if (sms != null)
                            {
                                var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;

                                simpleMarkerSymbol.Color = sms.Color;
                                simpleMarkerSymbol.Size = sms.Size;
                                simpleMarkerSymbol.Style = sms.Style;
                                simpleMarkerSymbol.OutlineSize = 1;

                                if (aiPoint.IsSelected)
                                {
                                    var color = new RgbColorClass() { Green = 255 } as IColor;
                                    // Marker symbols
                                    simpleMarkerSymbol.Outline = true;
                                    simpleMarkerSymbol.OutlineColor = color;
                                }
                                else
                                {
                                    simpleMarkerSymbol.Outline = false;
                                    //simpleMarkerSymbol.OutlineColor = sms.Color;
                                }

                                markerElement.Symbol = simpleMarkerSymbol;

                                gc.UpdateElement(element);
                            }
                        }
                    }
                }


                element = gc.Next();
            }

            av.Refresh();
        }

        private void AddCollectionPoint(IPoint point)
        {
            if (!point.IsEmpty && point != null)
            {
                var color = new RgbColorClass() { Red = 255 } as IColor;
                var guid = ArcMapHelpers.AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, 7);
                var addInPoint = new AddInPoint() { Point = point, GUID = guid };
                CoordinateAddInPoints.Add(addInPoint);

                GraphicsList.Add(new AMGraphic(guid, point, true));
            }
        }

        private void RemoveGraphics(List<string> guidList)
        {
            if (!guidList.Any())
                return;

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            if (mxdoc == null)
                return;
            var av = mxdoc.FocusMap as IActiveView;
            if (av == null)
                return;
            var gc = av as IGraphicsContainer;
            if (gc == null)
                return;

            var graphics = GraphicsList.Where(g => guidList.Contains(g.UniqueId)).ToList();
            RemoveGraphics(gc, graphics);

            av.Refresh();
        }


        private void DeletePoints(List<AddInPoint> aiPoints)
        {
            if (aiPoints == null || !aiPoints.Any())
                return;

            // remove graphics from map
            var guidList = aiPoints.Select(x => x.GUID).ToList();
            RemoveGraphics(guidList);

            foreach (var point in aiPoints)
            {
                CoordinateAddInPoints.Remove(point);
            }
        }

        private void OnSetListBoxItemAddInPoint(object obj)
        {
            ListBoxItemAddInPoint = obj as AddInPoint;
            RaisePropertyChanged(() => HasListBoxRightClickSelectedItem);
        }

        private void OnImportCoordinates(object obj)
        {
            var coordinates = obj as List<string>;

            if (coordinates == null)
                return;

            foreach (var coordinate in coordinates)
            {
                this.ProcessInput(coordinate);
                InputCoordinate = coordinate;
                if (!HasInputError)
                    OnNewMapPoint(amCoordGetter.Point);
            }
        }

        private void ClearGraphicsContainer(IMap map)
        {
            var graphicsContainer = map as IGraphicsContainer;
            if (graphicsContainer != null)
            {
                //graphicsContainer.DeleteAllElements();
                // now we have a collection feature and need to not clear those related graphics
                graphicsContainer.Reset();
                var g = graphicsContainer.Next();
                while (g != null)
                {
                    if (!CoordinateAddInPoints.Any(aiPoint => aiPoint.GUID == ((IElementProperties)g).Name))
                        graphicsContainer.DeleteElement(g);
                    g = graphicsContainer.Next();
                }
            }
        }


        #region overrides

        internal override void OnFlashPointCommand(object obj)
        {
            if (ListBoxItemAddInPoint != null)
            {
                var geometry = ListBoxItemAddInPoint.Point;
                ListBoxItemAddInPoint = null;

                base.OnFlashPointCommand(geometry);
            }
        }

        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            AddCollectionPoint(obj as IPoint);

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibrary.Models.CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            // update list box coordinates
            var list = CoordinateAddInPoints.ToList();
            CoordinateAddInPoints.Clear();
            foreach (var item in list)
                CoordinateAddInPoints.Add(item);
        }

        #endregion overrides
    }
}
