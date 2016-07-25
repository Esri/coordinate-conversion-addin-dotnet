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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;
using ArcMapAddinCoordinateConversion.Helpers;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class TabBaseViewModel : BaseViewModel
    {
        public TabBaseViewModel()
        {
            HasInputError = false;
            IsHistoryUpdate = true;
            IsToolGenerated = false;
            ToolMode = MapPointToolMode.Unknown;

            // commands
            ActivatePointToolCommand = new RelayCommand(OnActivatePointToolCommand);
            EditPropertiesDialogCommand = new RelayCommand(OnEditPropertiesDialogCommand);
            FlashPointCommand = new RelayCommand(OnFlashPointCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.TAB_ITEM_SELECTED, OnTabItemSelected);
            Mediator.Register(CoordinateConversionLibrary.Constants.NEW_MAP_POINT, OnNewMapPoint);
            Mediator.Register(CoordinateConversionLibrary.Constants.MOUSE_MOVE_POINT, OnMouseMove);
            Mediator.Register(CoordinateConversionLibrary.Constants.NewMapPointSelection, OnNewMapPointSelection);
            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            Mediator.Register(CoordinateConversionLibrary.Constants.SetToolMode, (mode) => 
            {
                MapPointToolMode eMode = MapPointToolMode.Unknown;
                Enum.TryParse<MapPointToolMode>(mode.ToString(), out eMode);
                ToolMode = eMode;
            });

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, amCoordGetter);

            configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
                .RegisterHandler(n => n.DisplayCoordinateType, n =>
                {
                    if (amCoordGetter != null && amCoordGetter.Point != null)
                    {
                        InputCoordinate = amCoordGetter.GetInputDisplayString();
                    }
                });
        }

        PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        public RelayCommand ActivatePointToolCommand { get; set; }
        public RelayCommand EditPropertiesDialogCommand { get; set; }
        public RelayCommand FlashPointCommand { get; set; }

        public MapPointToolMode ToolMode { get; set; }

        public CoordinateType InputCoordinateType { get; set; }
        public bool IsToolActive
        {
            get
            {
                if (ArcMap.Application.CurrentTool != null)
                    return ArcMap.Application.CurrentTool.Name == "ESRI_ArcMapAddinCoordinateConversion_MapPointTool";

                return false;
            }
            set
            {
                if (value)
                    OnActivatePointToolCommand(null);
                else
                    if (ArcMap.Application.CurrentTool != null)
                        ArcMap.Application.CurrentTool = null;

                RaisePropertyChanged(() => IsToolActive);
            }
        }

        private bool isActiveTab = false;
        /// <summary>
        /// Property to keep track of which tab/viewmodel is the active item
        /// </summary>
        public bool IsActiveTab
        {
            get
            {
                return isActiveTab;
            }
            set
            {
                //TODO do we need a reset?
                //Reset(true);
                isActiveTab = value;
                RaisePropertyChanged(() => IsActiveTab);
            }
        }


        private string _inputCoordinate;
        public string InputCoordinate
        {
            get
            {
                return _inputCoordinate;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                _inputCoordinate = value;
                var tempDD = ProcessInput(_inputCoordinate);
                //TODO where does this belong, virtual method call? Mediator call?
                // update tool view model
                //var ctvm = ConvertTabView.Resources["CTViewModel"] as CoordinateConversionViewModel;
                //if (ctvm != null)
                //{
                //    ctvm.SetCoordinateGetter(amCoordGetter);
                //    ctvm.InputCoordinate = tempDD;
                //    var formattedInputCoordinate = amCoordGetter.GetInputDisplayString();
                //    // update history
                //    if (IsHistoryUpdate)
                //    {
                //        if (IsToolGenerated)
                //        {
                //            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);
                //        }
                //        else
                //        {
                //            UIHelpers.UpdateHistory(_inputCoordinate, InputCoordinateHistoryList);
                //        }
                //    }
                //    // reset flags
                //    IsHistoryUpdate = true;
                //    IsToolGenerated = false;

                //    _inputCoordinate = formattedInputCoordinate;
                //}

                RaisePropertyChanged(() => InputCoordinate);
            }
        }
        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                RaisePropertyChanged(() => HasInputError);
            }
        }
        public bool IsHistoryUpdate { get; set; }
        public bool IsToolGenerated { get; set; }

        //TODO check if this is a good home for this
        public static ArcMapCoordinateGet amCoordGetter = new ArcMapCoordinateGet();
        
        internal void OnActivatePointToolCommand(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "ESRI_ArcMapAddinCoordinateConversion_MapPointTool");
        }

        internal void OnEditPropertiesDialogCommand(object obj)
        {
            var dlg = new EditPropertiesView();

            dlg.ShowDialog();
        }


        internal virtual void OnFlashPointCommand(object obj)
        {
            IGeometry address = null;

            //TODO override in ConvertTabViewModel
            //if (amCoordGetter != null && amCoordGetter.Point != null)
            //    address = amCoordGetter.Point;

            //TODO override in CollectTabViewModel
            //if (ListBoxItemAddInPoint != null)
            //{
            //    address = ListBoxItemAddInPoint.Point;
            //    ListBoxItemAddInPoint = null;
            //}

            if (address != null)
            {
                // Map und View
                IMxDocument mxdoc = ArcMap.Application.Document as IMxDocument;
                IActiveView activeView = mxdoc.ActivatedView;
                IMap map = mxdoc.FocusMap;
                IEnvelope envelope = activeView.Extent;

                //ClearGraphicsContainer(map);

                IScreenDisplay screenDisplay = activeView.ScreenDisplay;
                short screenCache = Convert.ToInt16(esriScreenCache.esriNoScreenCache);

                ISpatialReference outgoingCoordSystem = map.SpatialReference;
                address.Project(outgoingCoordSystem);

                // is point within current extent
                // if so, pan to point
                var relationOp = envelope as IRelationalOperator;
                if (relationOp != null && activeView is IMap)
                {
                    if (!relationOp.Contains(address))
                    {
                        // pan to
                        envelope.CenterAt(address as IPoint);
                        activeView.Extent = envelope;
                        activeView.Refresh();
                    }
                }

                IRgbColor color = new RgbColorClass();
                color.Green = 80;
                color.Red = 22;
                color.Blue = 68;

                ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
                simpleMarkerSymbol.Color = color;
                simpleMarkerSymbol.Size = 15;
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

                IElement element = null;

                IMarkerElement markerElement = new MarkerElementClass();

                markerElement.Symbol = simpleMarkerSymbol;
                element = markerElement as IElement;

                IPolygon poly = null;
                if (InputCoordinateType == CoordinateType.MGRS || InputCoordinateType == CoordinateType.USNG)
                {
                    poly = GetMGRSPolygon(address as IPoint);
                }

                if (poly != null)
                {
                    address = poly;
                }
                var av = mxdoc.FocusMap as IActiveView;
                FlashGeometry(address, color, av.ScreenDisplay, 500, av.Extent);

                //AddElement(map, address);

                // do not center if in layout view
                //if (mxdoc.ActiveView is IMap)
                //{
                //    if (poly != null && !poly.IsEmpty && (poly as IArea) != null)
                //        envelope.CenterAt((poly as IArea).Centroid);
                //    else
                //        envelope.CenterAt(amCoordGetter.Point);

                //    activeView.Extent = envelope;
                //    activeView.Refresh();
                //}
            }
        }

        /// <summary>
        /// Handler for the tab item selected event
        /// Helps keep track of which tab item/viewmodel is active
        /// </summary>
        /// <param name="obj">bool if selected or not</param>
        private void OnTabItemSelected(object obj)
        {
            if (obj == null)
                return;

            IsActiveTab = (obj == this);
        }

        internal virtual void OnNewMapPoint(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            amCoordGetter.Point = point;
            InputCoordinate = amCoordGetter.GetInputDisplayString();
        }

        internal virtual void OnMouseMove(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            amCoordGetter.Point = point;
            InputCoordinate = amCoordGetter.GetInputDisplayString();
        }

        private void OnNewMapPointSelection(object obj)
        {
            var point = obj as IPoint;

            if (point == null)
                return;

            var sr = ArcMapHelpers.GetGCS_WGS_1984_SR();

            point.Project(sr);

            InputCoordinate = string.Format("{0:0.0####} {1:0.0####}", point.Y, point.X);
        }

        private string ProcessInput(string input)
        {
            string result = string.Empty;
            ESRI.ArcGIS.Geometry.IPoint point;

            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            InputCoordinateType = GetCoordinateType(input, out point);

            if (InputCoordinateType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                amCoordGetter.Point = point;
                try
                {
                    if (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType == CoordinateConversionLibrary.CoordinateTypes.None)
                    {
                        result = string.Format("{0:0.0} {1:0.0}", point.Y, point.X);
                    }
                    else
                        result = (point as IConversionNotation).GetDDFromCoords(6);
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }

            return result;
        }

        private void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
        {
            ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
            ESRI.ArcGIS.esriSystem.UID commandID = new ESRI.ArcGIS.esriSystem.UIDClass();
            commandID.Value = toolName; // example: "esriArcMapUI.ZoomInTool";
            ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(commandID, false, false);

            if (commandItem != null)
                application.CurrentTool = commandItem;
        }

        private void OnBCNeeded(object obj)
        {
            if (amCoordGetter == null || amCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(amCoordGetter.Point);
        }

        private void BroadcastCoordinateValues(ESRI.ArcGIS.Geometry.IPoint point)
        {

            var dict = new Dictionary<CoordinateType, string>();
            if (point == null)
                return;

            var cn = point as IConversionNotation;

            if (cn == null)
                return;

            try
            {
                dict.Add(CoordinateType.DD, cn.GetDDFromCoords(6));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.DDM, cn.GetDDMFromCoords(6));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.DMS, cn.GetDMSFromCoords(6));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.GARS, cn.GetGARSFromCoords());
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.MGRS, cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.USNG, cn.GetUSNGFromCoords(5, true, false));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.UTM, cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces | esriUTMConversionOptionsEnum.esriUTMUseNS));
            }
            catch { }

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);
        }

        private IPolygon GetMGRSPolygon(IPoint point)
        {
            CoordinateMGRS mgrs;

            IPointCollection pc = new RingClass();

            // bottom left
            CoordinateMGRS.TryParse(InputCoordinate, out mgrs);

            // don't create a polygon for 1m resolution
            if (mgrs.Easting.ToString().Length > 4 && mgrs.Northing.ToString().Length > 4)
                return null;

            var tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();//GetSR();
            var anotherMGRSstring = mgrs.ToString("", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // top left
            var tempMGRS = new CoordinateMGRS(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
            var tempEasting = mgrs.Easting.ToString().PadRight(5, '0');
            tempMGRS.Easting = Convert.ToInt32(tempEasting);
            var tempNorthing = mgrs.Northing.ToString().PadRight(5, '9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            anotherMGRSstring = tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // top right
            tempEasting = mgrs.Easting.ToString().PadRight(5, '9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5, '9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // bottom right
            tempEasting = mgrs.Easting.ToString().PadRight(5, '9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5, '0');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing);

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // create polygon
            var poly = new PolygonClass();
            poly.SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            poly.AddPointCollection(pc);
            poly.Close();

            return poly;
        }

        ///<summary>Flash geometry on the display. The geometry type could be polygon, polyline, point, or multipoint.</summary>
        ///
        ///<param name="geometry"> An IGeometry interface</param>
        ///<param name="color">An IRgbColor interface</param>
        ///<param name="display">An IDisplay interface</param>
        ///<param name="delay">A System.Int32 that is the time im milliseconds to wait.</param>
        /// 
        ///<remarks></remarks>
        private void FlashGeometry(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IRgbColor color, ESRI.ArcGIS.Display.IDisplay display, System.Int32 delay, IEnvelope envelope)
        {
            if (geometry == null || color == null || display == null)
            {
                return;
            }

            display.StartDrawing(display.hDC, (System.Int16)ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit Cast

            switch (geometry.GeometryType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    {
                        //Set the flash geometry's symbol.
                        ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new ESRI.ArcGIS.Display.SimpleFillSymbolClass();
                        simpleFillSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol symbol = simpleFillSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        //Flash the input polygon geometry.
                        display.SetSymbol(symbol);
                        display.DrawPolygon(geometry);
                        System.Threading.Thread.Sleep(delay);
                        display.DrawPolygon(geometry);
                        break;
                    }

                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    {
                        //Set the flash geometry's symbol.
                        ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
                        simpleLineSymbol.Width = 4;
                        simpleLineSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol symbol = simpleLineSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        //Flash the input polyline geometry.
                        display.SetSymbol(symbol);
                        display.DrawPolyline(geometry);
                        System.Threading.Thread.Sleep(delay);
                        display.DrawPolyline(geometry);
                        break;
                    }

                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    {
                        //Set the flash geometry's symbol.
                        ESRI.ArcGIS.Display.ISimpleMarkerSymbol simpleMarkerSymbol = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
                        simpleMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
                        simpleMarkerSymbol.Size = 12;
                        simpleMarkerSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol markerSymbol = simpleMarkerSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        markerSymbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
                        simpleLineSymbol.Width = 1;
                        simpleLineSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol lineSymbol = simpleLineSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        lineSymbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        //Flash the input polygon geometry.
                        display.SetSymbol(markerSymbol);
                        display.SetSymbol(lineSymbol);

                        DrawCrossHair(geometry, display, envelope, markerSymbol, lineSymbol);

                        //Flash the input point geometry.
                        display.SetSymbol(markerSymbol);
                        display.DrawPoint(geometry);
                        System.Threading.Thread.Sleep(delay);
                        display.DrawPoint(geometry);
                        break;
                    }

                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
                    {
                        //Set the flash geometry's symbol.
                        ESRI.ArcGIS.Display.ISimpleMarkerSymbol simpleMarkerSymbol = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
                        simpleMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
                        simpleMarkerSymbol.Size = 12;
                        simpleMarkerSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol symbol = simpleMarkerSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        //Flash the input multipoint geometry.
                        display.SetSymbol(symbol);
                        display.DrawMultipoint(geometry);
                        System.Threading.Thread.Sleep(delay);
                        display.DrawMultipoint(geometry);
                        break;
                    }
            }

            display.FinishDrawing();
        }

        private static void DrawCrossHair(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IDisplay display, IEnvelope extent, ISymbol markerSymbol, ISymbol lineSymbol)
        {
            var point = geometry as IPoint;
            var numSegments = 10;

            var latitudeMid = point.Y;//envelope.YMin + ((envelope.YMax - envelope.YMin) / 2);
            var longitudeMid = point.X;
            var leftLongSegment = (point.X - extent.XMin) / numSegments;
            var rightLongSegment = (extent.XMax - point.X) / numSegments;
            var topLatSegment = (extent.YMax - point.Y) / numSegments;
            var bottomLatSegment = (point.Y - extent.YMin) / numSegments;
            var fromLeftLong = extent.XMin;
            var fromRightLong = extent.XMax;
            var fromTopLat = extent.YMax;
            var fromBottomLat = extent.YMin;
            var av = (ArcMap.Application.Document as IMxDocument).ActiveView;

            var leftPolyline = new PolylineClass();
            var rightPolyline = new PolylineClass();
            var topPolyline = new PolylineClass();
            var bottomPolyline = new PolylineClass();

            leftPolyline.SpatialReference = geometry.SpatialReference;
            rightPolyline.SpatialReference = geometry.SpatialReference;
            topPolyline.SpatialReference = geometry.SpatialReference;
            bottomPolyline.SpatialReference = geometry.SpatialReference;

            var leftPC = leftPolyline as IPointCollection;
            var rightPC = rightPolyline as IPointCollection;
            var topPC = topPolyline as IPointCollection;
            var bottomPC = bottomPolyline as IPointCollection;

            leftPC.AddPoint(new PointClass() { X = fromLeftLong, Y = latitudeMid });
            rightPC.AddPoint(new PointClass() { X = fromRightLong, Y = latitudeMid });
            topPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromTopLat });
            bottomPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromBottomLat });

            for (int x = 1; x <= numSegments; x++)
            {
                //Flash the input polygon geometry.
                display.SetSymbol(markerSymbol);
                display.SetSymbol(lineSymbol);

                leftPC.AddPoint(new PointClass() { X = fromLeftLong + leftLongSegment * x, Y = latitudeMid });
                rightPC.AddPoint(new PointClass() { X = fromRightLong - rightLongSegment * x, Y = latitudeMid });
                topPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromTopLat - topLatSegment * x });
                bottomPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromBottomLat + bottomLatSegment * x });

                // draw
                display.DrawPolyline(leftPolyline);
                display.DrawPolyline(rightPolyline);
                display.DrawPolyline(topPolyline);
                display.DrawPolyline(bottomPolyline);

                System.Threading.Thread.Sleep(15);
                display.FinishDrawing();
                av.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                //av.Refresh();
                System.Windows.Forms.Application.DoEvents();
                display.StartDrawing(display.hDC, (System.Int16)ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit Cast
            }
        }

        private CoordinateType GetCoordinateType(string input, out ESRI.ArcGIS.Geometry.IPoint point)
        {
            point = new PointClass();
            var cn = point as IConversionNotation;
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            point.SpatialReference = geographicCS;

            try
            {
                cn.PutCoordsFromDD(input);
                return CoordinateType.DD;
            }
            catch { }

            try
            {
                cn.PutCoordsFromDDM(input);
                return CoordinateType.DDM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromDMS(input);
                return CoordinateType.DMS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, input);
                return CoordinateType.GARS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeLL, input);
                return CoordinateType.GARS;
            }
            catch { }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, gars.ToString("", new CoordinateGARSFormatter()));
                    return CoordinateType.GARS;
                }
                catch { }
            }

            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewStyle);
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewWith180InZone01);
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldStyle);
                return CoordinateType.MGRS;
            }
            catch { }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldWith180InZone01);
                return CoordinateType.MGRS;
            }
            catch { }

            // mgrs try parse
            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    cn.PutCoordsFromMGRS(mgrs.ToString("", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_NewStyle);
                    return CoordinateType.MGRS;
                }
                catch { }
            }

            try
            {
                cn.PutCoordsFromUSNG(input);
                return CoordinateType.USNG;
            }
            catch { }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    cn.PutCoordsFromUSNG(usng.ToString("", new CoordinateMGRSFormatter()));
                    return CoordinateType.USNG;
                }
                catch { }
            }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMAddSpaces, input);
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, input);
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMUseNS, input);
                return CoordinateType.UTM;
            }
            catch { }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, utm.ToString("", new CoordinateUTMFormatter()));
                    return CoordinateType.UTM;
                }
                catch { }
            }

            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    point.X = Lon;
                    point.Y = Lat;
                    point.SpatialReference = ArcMapHelpers.GetSR((int)esriSRProjCS3Type.esriSRProjCS_WGS1984WebMercatorMajorAuxSphere);
                    return CoordinateType.DD;
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }

            return CoordinateType.Unknown;
        }

    }
}
