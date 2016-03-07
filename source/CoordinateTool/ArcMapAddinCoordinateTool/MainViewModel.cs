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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Views;
using CoordinateToolLibrary.ViewModels;
using ESRI.ArcGIS.Geometry;
using System.Windows;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Helpers;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Display;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace ArcMapAddinCoordinateTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            _coordinateToolView = new CoordinateToolView();
            HasInputError = false;
            IsHistoryUpdate = false;
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            ActivatePointToolCommand = new RelayCommand(OnActivatePointToolCommand);
            FlashPointCommand = new RelayCommand(OnFlashPointCommand);
            CopyAllCommand = new RelayCommand(OnCopyAllCommand);
            Mediator.Register(CoordinateToolLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            InputCoordinateHistoryList = new ObservableCollection<string>();

            // update tool view model
            var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
            if (ctvm != null)
            {
                ctvm.SetCoordinateGetter(amCoordGetter);
            }
        }

        public bool IsToolActive
        {
            get
            {
                if(ArcMap.Application.CurrentTool != null)
                    return ArcMap.Application.CurrentTool.Name == "ESRI_ArcMapAddinCoordinateConversion_PointTool";

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

        public CoordinateType InputCoordinateType { get; set; }
        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        private void OnCopyAllCommand(object obj)
        {
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.CopyAllCoordinateOutputs, InputCoordinate);
        }
        private ISpatialReference GetSR()
        {
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            return geographicCS as ISpatialReference;
        }
        private void OnFlashPointCommand(object obj)
        {
            if(amCoordGetter != null && amCoordGetter.Point != null)
            {
                IGeometry address = amCoordGetter.Point;

                // Map und View
                IMxDocument mxdoc = ArcMap.Application.Document as IMxDocument;
                IActiveView activeView = mxdoc.ActivatedView;
                IMap map = mxdoc.FocusMap;
                IEnvelope envelope = activeView.Extent;

                ClearGraphicsContainer(map);

                IScreenDisplay screenDisplay = activeView.ScreenDisplay;
                short screenCache = Convert.ToInt16(esriScreenCache.esriNoScreenCache);

                ISpatialReference outgoingCoordSystem = map.SpatialReference;
                address.Project(outgoingCoordSystem);

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
                FlashGeometry(address, color, av.ScreenDisplay, 500);

                AddElement(map, address);

                // do not center if in layout view
                if (mxdoc.ActiveView is IMap)
                {
                    if (poly != null && !poly.IsEmpty && (poly as IArea) != null)
                        envelope.CenterAt((poly as IArea).Centroid);
                    else
                        envelope.CenterAt(amCoordGetter.Point);

                    activeView.Extent = envelope;
                    activeView.Refresh();
                }
            }
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
            (tempPoint as IPoint).SpatialReference = GetSR();
            var anotherMGRSstring = mgrs.ToString("", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);
            
            // top left
            var tempMGRS = new CoordinateMGRS(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
            var tempEasting = mgrs.Easting.ToString().PadRight(5,'0');
            tempMGRS.Easting = Convert.ToInt32(tempEasting);
            var tempNorthing = mgrs.Northing.ToString().PadRight(5,'9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0','9'));

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = GetSR();
            anotherMGRSstring = tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // top right
            tempEasting = mgrs.Easting.ToString().PadRight(5,'9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5,'9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = GetSR();
            tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // bottom right
            tempEasting = mgrs.Easting.ToString().PadRight(5,'9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5,'0');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing);

            tempPoint = new PointClass() as IConversionNotation;
            (tempPoint as IPoint).SpatialReference = GetSR();
            tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // create polygon
            var poly = new PolygonClass();
            poly.SpatialReference = GetSR();
            poly.AddPointCollection(pc);
            poly.Close();

            return poly;
        }
        private void AddElement(IMap map, IGeometry geom)
        {
            IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
            IRgbColor color = new RgbColorClass();
            color.Green = 80;
            color.Red = 22;
            color.Blue = 68;

            IElement element = null;

            if (geom is IPoint)
            {
                ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
                simpleMarkerSymbol.Color = color;
                simpleMarkerSymbol.Size = 15;
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

                IMarkerElement markerElement = new MarkerElementClass();

                markerElement.Symbol = simpleMarkerSymbol;
                element = markerElement as IElement;

                if (element != null)
                {
                    element.Geometry = geom;
                }
            }
            else if(geom is IPolygon)
            {
                var temp = new SimpleLineSymbol();
                temp.Color = color;
                temp.Style = esriSimpleLineStyle.esriSLSSolid;
                temp.Width = 2;
                var s = new SimpleFillSymbol();
                s.Color = color;
                s.Outline = temp;
                s.Style = esriSimpleFillStyle.esriSFSBackwardDiagonal;

                var pe = new PolygonElementClass();
                element = pe as IElement;
                var fill = pe as IFillShapeElement;
                fill.Symbol = s;
                element.Geometry = geom;
            }
            graphicsContainer.AddElement(element, 0);
            IActiveView activeView = map as IActiveView;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void ClearGraphicsContainer(IMap map)
        {
            var graphicsContainer = map as IGraphicsContainer;
            if (graphicsContainer != null)
                graphicsContainer.DeleteAllElements();
        }
        private void AddElement(IMap map, IPoint point)
        {
            IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
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

            if (element != null)
            {
                element.Geometry = point;
            }
            graphicsContainer.AddElement(element, 0);

            //Flag the new text to invalidate.
            IActiveView activeView = map as IActiveView;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void AddTextElement(IMap map, IPoint point, string text)
        {
            IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
            IElement element = new TextElementClass();
            ITextElement textElement = element as ITextElement;

            element.Geometry = point;
            textElement.Text = text;
            graphicsContainer.AddElement(element, 0);

            //Flag the new text to invalidate.
            IActiveView activeView = map as IActiveView;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void OnActivatePointToolCommand(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "ESRI_ArcMapAddinCoordinateConversion_PointTool");
        }

        public void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
        {
            ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
            ESRI.ArcGIS.esriSystem.UID commandID = new ESRI.ArcGIS.esriSystem.UIDClass();
            commandID.Value = toolName; // example: "esriArcMapUI.ZoomInTool";
            ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(commandID, false, false);

            if (commandItem != null)
                application.CurrentTool = commandItem;
        }

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            string name = CoordinateType.DD.ToString();
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.AddNewOutputCoordinate, new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#N X0.0#E" });
        }

        private ArcMapCoordinateGet amCoordGetter = new ArcMapCoordinateGet();

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

        public RelayCommand AddNewOCCommand { get; set; }
        public RelayCommand ActivatePointToolCommand { get; set; }
        public RelayCommand FlashPointCommand { get; set; }
        public RelayCommand CopyAllCommand { get; set; }

        public bool IsHistoryUpdate { get; set; }

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

                // update tool view model
                var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
                if (ctvm != null)
                {
                    ctvm.SetCoordinateGetter(amCoordGetter);
                    ctvm.InputCoordinate = tempDD;
                }

                RaisePropertyChanged(() => InputCoordinate);
            }
        }

        public void UpdateButtonState()
        {
            RaisePropertyChanged(() => IsToolActive);
        }
        private CoordinateToolView _coordinateToolView;
        public CoordinateToolView CTView
        {
            get
            {
                return _coordinateToolView;
            }
            set
            {
                _coordinateToolView = value;
            }
        }

        internal string GetFormattedCoordinate(string coord, CoordinateType cType)
        {
            string format = "";

            var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolLibrary.ViewModels.CoordinateToolViewModel;
            if (ctvm != null)
            {
                var ocvm = ctvm.OCView.DataContext as CoordinateToolLibrary.ViewModels.OutputCoordinateViewModel;

                if (ocvm != null)
                {
                    var tt = ocvm.OutputCoordinateList.FirstOrDefault(t => t.CType == cType);
                    if (tt != null)
                    {
                        format = tt.Format;
                        Console.WriteLine(tt.Format);
                    }
                }
            }

            var cf = CoordinateHandler.GetFormattedCoord(cType, coord, format);

            if (!String.IsNullOrWhiteSpace(cf))
                return cf;

            return string.Empty;
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
                result = (point as IConversionNotation).GetDDFromCoords(6);
                if (IsHistoryUpdate)
                {
                    UIHelpers.UpdateHistory(input, InputCoordinateHistoryList);
                    IsHistoryUpdate = false;
                }
            }

            return result;
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
            if(CoordinateGARS.TryParse(input, out gars))
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
            if(CoordinateUSNG.TryParse(input, out usng))
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
            if(CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, utm.ToString("", new CoordinateUTMFormatter()));
                    return CoordinateType.UTM;
                }
                catch { }
            }

            return CoordinateType.Unknown;
        }

        private void OnBCNeeded(object obj)
        {
            if(amCoordGetter == null || amCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(amCoordGetter.Point);
        }

        private void BroadcastCoordinateValues(ESRI.ArcGIS.Geometry.IPoint point)
        {

            var dict = new Dictionary<CoordinateType, string>();
            if (point == null)
                return;

            var cn = point as IConversionNotation;

            if(cn == null)
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
                dict.Add(CoordinateType.UTM, cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS));
            }
            catch { }

            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.BroadcastCoordinateValues, dict);
        }

        ///<summary>Flash geometry on the display. The geometry type could be polygon, polyline, point, or multipoint.</summary>
        ///
        ///<param name="geometry"> An IGeometry interface</param>
        ///<param name="color">An IRgbColor interface</param>
        ///<param name="display">An IDisplay interface</param>
        ///<param name="delay">A System.Int32 that is the time im milliseconds to wait.</param>
        /// 
        ///<remarks></remarks>
        private void FlashGeometry(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IRgbColor color, ESRI.ArcGIS.Display.IDisplay display, System.Int32 delay)
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
                        simpleMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSDiamond;
                        simpleMarkerSymbol.Size = 12;
                        simpleMarkerSymbol.Color = color;
                        ESRI.ArcGIS.Display.ISymbol symbol = simpleMarkerSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
                        symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

                        //Flash the input point geometry.
                        display.SetSymbol(symbol);
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

    }

    public class BoolToOppositeBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
