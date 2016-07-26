﻿// Copyright 2016 Esri 
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

using System;
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

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class CollectTabViewModel : TabBaseViewModel
    {
        public CollectTabViewModel()
        {
            ListBoxItemAddInPoint = null;

            CoordinateAddInPoints = new ObservableCollection<AddInPoint>();

            DeletePointCommand = new RelayCommand(OnDeletePointCommand);
            DeleteAllPointsCommand = new RelayCommand(OnDeleteAllPointsCommand);
            ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
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
            //TODO check to see if we still need this
            //RaisePropertyChanged(() => HasMapGraphics);
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
            var color = new RgbColorClass() { Red = 255 } as IColor;
            var guid = AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, 7);
            var addInPoint = new AddInPoint() { Point = point, GUID = guid };
            CoordinateAddInPoints.Add(addInPoint);
        }

        /// <summary>
        /// Adds a graphic element to the map graphics container
        /// </summary>
        /// <param name="geom">IGeometry</param>
        private string AddGraphicToMap(IGeometry geom, IColor color, bool IsTempGraphic = false, esriSimpleMarkerStyle markerStyle = esriSimpleMarkerStyle.esriSMSCircle, int size = 5)
        {
            if (geom == null || ArcMap.Document == null || ArcMap.Document.FocusMap == null)
                return string.Empty;

            IElement element = null;
            double width = 2.0;

            geom.Project(ArcMap.Document.FocusMap.SpatialReference);

            if (geom.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                // Marker symbols
                var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;
                simpleMarkerSymbol.Color = color;
                simpleMarkerSymbol.Outline = false;
                simpleMarkerSymbol.OutlineColor = color;
                simpleMarkerSymbol.Size = size;
                simpleMarkerSymbol.Style = markerStyle;

                var markerElement = new MarkerElement() as IMarkerElement;
                markerElement.Symbol = simpleMarkerSymbol;
                element = markerElement as IElement;
            }
            else if (geom.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                // create graphic then add to map
                var le = new LineElementClass() as ILineElement;
                element = le as IElement;

                var lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = color;
                lineSymbol.Width = width;

                le.Symbol = lineSymbol;
            }
            else if (geom.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                // create graphic then add to map
                IPolygonElement pe = new PolygonElementClass() as IPolygonElement;
                element = pe as IElement;
                IFillShapeElement fe = pe as IFillShapeElement;

                var fillSymbol = new SimpleFillSymbolClass();
                RgbColor selectedColor = new RgbColorClass();
                selectedColor.Red = 0;
                selectedColor.Green = 0;
                selectedColor.Blue = 0;

                selectedColor.Transparency = (byte)0;
                fillSymbol.Color = selectedColor;

                fe.Symbol = fillSymbol;
            }

            if (element == null)
                return string.Empty;

            element.Geometry = geom;

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var gc = av as IGraphicsContainer;

            // store guid
            var eprop = element as IElementProperties;
            eprop.Name = Guid.NewGuid().ToString();

            GraphicsList.Add(new AMGraphic(eprop.Name, geom, IsTempGraphic));

            gc.AddElement(element, 0);

            av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            //TODO check to see if this is still needed
            //RaisePropertyChanged(() => HasMapGraphics);

            return eprop.Name;
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

        internal override void OnNewMapPoint(object obj)
        {
            base.OnNewMapPoint(obj);

            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            var color = new RgbColorClass() { Red = 255 } as IColor;
            var guid = AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, 7);
            var addInPoint = new AddInPoint() { Point = point, GUID = guid };
            CoordinateAddInPoints.Add(addInPoint);
        }

        internal override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibrary.Models.CoordinateConversionLibraryConfig obj)
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