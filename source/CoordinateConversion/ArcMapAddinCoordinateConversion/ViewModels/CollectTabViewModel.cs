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
using System;

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
            PasteCoordinatesCommand = new RelayCommand(OnPasteCommand);

            // Listen to collection changed event and notify colleagues
            CoordinateAddInPoints.CollectionChanged += CoordinateAddInPoints_CollectionChanged;

            Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
            Mediator.Register(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, OnImportCoordinates);
        }

        /// <summary>
        /// Notify if collection list has any items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoordinateAddInPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.CollectListHasItems, CoordinateAddInPoints.Any());
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

        public static ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }

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
                var addinPoint = CoordinateAddInPoints.Where(x => x.IsSelected).FirstOrDefault();
                amCoordGetter.Point = addinPoint.Point;
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);
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
                //UpdateHighlightedGraphics();
            }
        }

        public RelayCommand DeletePointCommand { get; set; }
        public RelayCommand DeleteAllPointsCommand { get; set; }
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand EnterKeyCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand CopyCoordinateCommand { get; set; }
        public RelayCommand CopyAllCoordinatesCommand { get; set; }
        public RelayCommand PasteCoordinatesCommand { get; set; }

        // lists to store GUIDs of graphics, temp feedback and map graphics
        internal static List<AMGraphic> GraphicsList = new List<AMGraphic>();

        private void OnDeletePointCommand(object obj)
        {
            var items = obj as IList;
            if (items == null)
                return;

            var objects = items.Cast<AddInPoint>().ToList();

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
        public override void OnCopyCommand(object obj)
        {
            var items = obj as IList;
            if (items == null)
                return;

            var objects = items.Cast<AddInPoint>().ToList();

            if (!objects.Any())
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
        }

        private void OnEnterKeyCommand(object obj)
        {
            if (!HasInputError && InputCoordinate.Length > 0)
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
                        var grpList = GetMapPointExportFormat(GraphicsList);
                        SaveAsType saveType = System.IO.Path.GetExtension(path).Equals(".shp") ? SaveAsType.Shapefile : SaveAsType.FileGDB;
                        fc = fcUtils.CreateFCOutput(path, saveType, grpList, ArcMap.Document.FocusMap.SpatialReference);
                    }
                }
                else if (vm.KmlIsChecked)
                {
                    path = PromptSaveFileDialog("kmz", "KMZ File (*.kmz)|*.kmz", CoordinateConversionLibrary.Properties.Resources.KMLLocationMessage);
                    if (path != null)
                    {
                        string kmlName = System.IO.Path.GetFileName(path);
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        string tempShapeFile = folderName + System.IO.Path.DirectorySeparatorChar +
                            "tmpShapefile.shp";
                        var grpList = GetMapPointExportFormat(GraphicsList);
                        IFeatureClass tempFc = fcUtils.CreateFCOutput(tempShapeFile, SaveAsType.Shapefile, grpList, ArcMap.Document.FocusMap.SpatialReference);

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
                        if (!aiPoints.Any())
                            return;
                        var csvExport = new CsvExport();
                        var displayAmb = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
                        foreach (var point in aiPoints)
                        {
                            var results = GetOutputFormats(point);
                            csvExport.AddRow();
                            foreach (var item in results)
                                csvExport[item.Key] = item.Value;
                            if (point.FieldsDictionary != null)
                            {
                                foreach (KeyValuePair<string, Tuple<object, bool>> item in point.FieldsDictionary)
                                {
                                    if (item.Key != PointFieldName && item.Key != OutputFieldName)
                                        csvExport[item.Key] = item.Value.Item1;
                                }
                            }
                            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;
                        }
                        CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = displayAmb;
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

        private List<CCAMGraphic> GetMapPointExportFormat(List<AMGraphic> mapPointList)
        {
            List<CCAMGraphic> results = new List<CCAMGraphic>();
            var columnCollection = new List<string>();
            foreach (var point in mapPointList)
            {
                var pt = point.Geometry as IPoint;
                if (pt == null)
                    continue;

                var attributes = GetOutputFormats(new AddInPoint() { Point = pt });
                if (point.FieldsDictionary != null)
                {
                    foreach (KeyValuePair<string, Tuple<object, bool>> item in point.FieldsDictionary)
                        if (item.Key != PointFieldName && item.Key != OutputFieldName)
                        {
                            attributes[item.Key] = Convert.ToString(item.Value.Item1);
                            if (!columnCollection.Contains(item.Key))
                                columnCollection.Add(item.Key);
                        }
                }
                CCAMGraphic ccMapPoint = new CCAMGraphic() { Attributes = attributes, MapPoint = point };
                results.Add(ccMapPoint);
            }
            foreach (var item in results)
            {
                foreach (var column in columnCollection)
	            {
                    if(!item.Attributes.ContainsKey(column))
                    {
                        item.Attributes.Add(column, null);
                    }
	            }                
            }
            return results;
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

            IGeoFeatureLayer geoLayer = (IGeoFeatureLayer)outputFeatureLayer;
            if (geoLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
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
                sfDlg.OverwritePrompt = true;
            }

            sfDlg.FileName = "";
            sfDlg.DefaultExt = ext;
            sfDlg.Filter = filter;
            sfDlg.Title = title;

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
                var eleProps = (IElementProperties)element;
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
            if ((ArcMap.Document == null) && (ArcMap.Document.FocusMap == null))
                return;

            var av = (IActiveView)ArcMap.Document.FocusMap;
            var gc = (IGraphicsContainer)av;

            gc.Reset();
            var element = gc.Next();

            while (element != null)
            {
                var eProp = (IElementProperties)element;

                if (eProp != null)
                {
                    var aiPoint = CoordinateAddInPoints.FirstOrDefault(p => p.GUID == eProp.Name);
                    var doUpdate = false;
                    if (aiPoint != null)
                    {
                        // highlight
                        var markerElement = element as IMarkerElement;
                        if (markerElement != null)
                        {
                            var sms = markerElement.Symbol as ISimpleMarkerSymbol;
                            if (sms != null)
                            {
                                var simpleMarkerSymbol = (ISimpleMarkerSymbol)new SimpleMarkerSymbol();

                                simpleMarkerSymbol.Color = sms.Color;
                                simpleMarkerSymbol.Size = ArcMapHelpers.DefaultMarkerSize;
                                simpleMarkerSymbol.Style = sms.Style;
                                simpleMarkerSymbol.OutlineSize = 1.7;

                                if (aiPoint.IsSelected)
                                {
                                    var color = (IColor)new RgbColorClass() { Green = 255 };
                                    // Marker symbols
                                    simpleMarkerSymbol.Size = ArcMapHelpers.DefaultMarkerSize + ArcMapHelpers.DefaultOutlineSize;
                                    simpleMarkerSymbol.Outline = true;
                                    simpleMarkerSymbol.OutlineColor = color;
                                    doUpdate = true;
                                }
                                else if (markerElement.Symbol.Size > 0)
                                {
                                    simpleMarkerSymbol.Outline = false;
                                    doUpdate = true;
                                    //simpleMarkerSymbol.OutlineColor = sms.Color;
                                }

                                if (doUpdate)
                                {
                                    markerElement.Symbol = simpleMarkerSymbol;
                                    gc.UpdateElement(element);
                                }
                            }
                        }
                    }
                }


                element = gc.Next();
            }

            av.Refresh();
        }

        private void AddCollectionPoint(IPoint point, Dictionary<string, Tuple<object, bool>> fieldsDictionary = null)
        {
            if (point != null && !point.IsEmpty)
            {
                var color = (IColor)new RgbColorClass() { Red = 255 };
                var guid = ArcMapHelpers.AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, ArcMapHelpers.DefaultMarkerSize);
                var addInPoint = new AddInPoint() { Point = point, GUID = guid, FieldsDictionary = fieldsDictionary };

                //Add point to the top of the list
                CoordinateAddInPoints.Insert(0, addInPoint);
                GraphicsList.Add(new AMGraphic(guid, point, true, fieldsDictionary));
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
            if (obj == null)
                return;
            var input = obj as List<Dictionary<string, Tuple<object, bool>>>;
            if (input != null)
                foreach (var item in input)
                {
                    var coordinate = item.Where(x => x.Key == OutputFieldName).Select(x => Convert.ToString(x.Value.Item1)).FirstOrDefault();
                    if (coordinate == "" || item.Where(x => x.Key == PointFieldName).Any())
                        continue;
                    this.ProcessInput(coordinate);
                    InputCoordinate = coordinate;
                    if (!HasInputError)
                    {
                        item.Add(PointFieldName, Tuple.Create((object)amCoordGetter.Point, false));
                        OnNewMapPoint(item);
                    }
                }
            else
            {
                List<string> coordinates = obj as List<string>;
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
            RaisePropertyChanged(() => CoordinateAddInPoints);
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

        public override void OnPasteCommand(object obj)
        {
            var input = Clipboard.GetText().Trim();
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var coordinates = new List<string>();
            foreach (var item in lines)
            {
                if (item.Trim() == "")
                    continue;
                var sb = new StringBuilder();
                sb.Append(item.Trim());
                coordinates.Add(sb.ToString());
            }

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, coordinates);
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

            var input = obj as Dictionary<string, Tuple<object, bool>>;
            IPoint point = (input != null) ? input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault() as IPoint : obj as IPoint;
            AddCollectionPoint(point as IPoint, input);

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

        public override void OnMapPointSelection(object obj)
        {
            var mapPoint = obj as IPoint;
            mapPoint.Project(ArcMap.Document.FocusMap.SpatialReference);
            ITopologicalOperator topoOp = (ITopologicalOperator)mapPoint;
            IGeometry p2Bufferd = topoOp.Buffer(20000);
            AddInPoint closestPoint = null;
            Double distance = 0;

            if (p2Bufferd == null)
                return;

            foreach (var item in CollectTabViewModel.CoordinateAddInPoints)
            {
                IRelationalOperator rel1 = (IRelationalOperator)item.Point;
                if (rel1.Within(p2Bufferd))
                {
                    var resultDistance = ((IProximityOperator)mapPoint).ReturnDistance((IGeometry)item.Point);
                    distance = (distance < resultDistance && distance > 0) ? distance : resultDistance;

                    if (resultDistance == distance)
                    {
                        closestPoint = item;
                        distance = resultDistance;
                    }
                }
            }
            if (closestPoint != null)
            {
                closestPoint.IsSelected = true;
                ListBoxSelectedItem = closestPoint;
            }
            RaisePropertyChanged(() => ListBoxSelectedItem);
        }

        #endregion overrides
    }
}
