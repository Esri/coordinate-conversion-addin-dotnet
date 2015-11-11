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

namespace ArcMapAddinCoordinateTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            _coordinateToolView = new CoordinateToolView();
            HasInputError = false;
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            ActivatePointToolCommand = new RelayCommand(OnActivatePointToolCommand);
            FlashPointCommand = new RelayCommand(OnFlashPointCommand);
            CopyAllCommand = new RelayCommand(OnCopyAllCommand);
            ExpandCommand = new RelayCommand(OnExpandCommand);
            Mediator.Register("BROADCAST_COORDINATE_NEEDED", OnBCNeeded);
            InputCoordinateHistoryList = new ObservableCollection<string>();
        }

        private void OnExpandCommand(object obj)
        {
            if (SelectedInputItemVisibility == Visibility.Visible)
                SelectedInputItemVisibility = Visibility.Collapsed;
            else
                SelectedInputItemVisibility = Visibility.Visible;
        }
        public RelayCommand ExpandCommand { get; set; }
        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        private void OnCopyAllCommand(object obj)
        {
            Mediator.NotifyColleagues("COPY_ALL_COORDINATE_OUTPUTS", null);
        }
        private ISpatialReference GetSR()
        {
            // create wgs84 spatial reference
            //var spatialFactory = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironmentClass();
            //var temp = spatialFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            //return temp as ISpatialReference;

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

                IPoint address = amCoordGetter.Point;//new PointClass();

                // Map und View
                IMxDocument mxdoc = ArcMap.Application.Document as IMxDocument;
                IActiveView activeView = mxdoc.ActivatedView;
                IMap map = mxdoc.FocusMap;
                IEnvelope envelope = activeView.Extent;

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

                if (element != null)
                {
                    element.Geometry = address;
                }

                
                ESRI.ArcGIS.Carto.IGraphicsLayer graphicsLayer = new CompositeGraphicsLayerClass();
                ((ILayer)graphicsLayer).Name = "Flashy Coordinates Layer";
                ((ILayer)graphicsLayer).SpatialReference = GetSR();
                (graphicsLayer as IGraphicsContainer).AddElement(element, 0);

                FlashGeometry(address, color, mxdoc.ActiveView.ScreenDisplay, 500);
                
                envelope.CenterAt(address);
                activeView.Extent = envelope;
                activeView.Refresh();
            }
        }

        private void OnActivatePointToolCommand(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "ESRI_ArcMapAddinCoordinateTool_PointTool");
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
            string name = "Temp";
            Mediator.NotifyColleagues("AddNewOutputCoordinate", new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y#.##N X#.##E" });
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

        private Visibility _selectedInputItemVisibility = Visibility.Collapsed;
        public Visibility SelectedInputItemVisibility
        {
            get { return _selectedInputItemVisibility; }
            set
            {
                _selectedInputItemVisibility = value;
                RaisePropertyChanged(() => SelectedInputItemVisibility);
            }
        }
        private string _selectedInputItem = "";
        public string SelectedInputItem
        {
            get { return _selectedInputItem; }
            set
            {
                _selectedInputItem = value;
                RaisePropertyChanged(() => SelectedInputItem);
            }
        }

        private CoordinateGARS _inputGARS = new CoordinateGARS();
        public CoordinateGARS InputGARS
        {
            get { return _inputGARS; }
            set
            {
                _inputGARS = value;
                RaisePropertyChanged(() => InputGARS);
            }
        }
        private CoordinateMGRS _inputMGRS = new CoordinateMGRS();
        public CoordinateMGRS InputMGRS
        {
            get { return _inputMGRS; }
            set
            {
                _inputMGRS = value;
                RaisePropertyChanged(() => InputMGRS);
            }
        }
        private CoordinateUSNG _inputUSNG = new CoordinateUSNG();
        public CoordinateUSNG InputUSNG
        {
            get { return _inputUSNG; }
            set
            {
                _inputUSNG = value;
                RaisePropertyChanged(() => InputUSNG);
            }
        }
        private CoordinateUTM _inputUTM = new CoordinateUTM();
        public CoordinateUTM InputUTM
        {
            get { return _inputUTM; }
            set
            {
                _inputUTM = value;
                RaisePropertyChanged(() => InputUTM);
            }
        }

        private string _inputUTMZoneWithHemi = string.Empty;
        public string InputUTMZoneWithHemi
        {
            get { return string.Format("{0}{1}", InputUTM.Zone, InputUTM.Hemi); }
            set
            {
                // TODO better validation, regex, etc
                if(value.Length > 1 && (value[value.Length-1] == 'N' || value[value.Length-1] == 'S'))
                {
                    InputUTM.Hemi = string.Format("{0}", value[value.Length - 1]);
                    var temp = value.Replace(InputUTM.Hemi,"");
                    InputUTM.Zone = Convert.ToInt32(temp);
                }
            }
        }

        public RelayCommand AddNewOCCommand { get; set; }
        public RelayCommand ActivatePointToolCommand { get; set; }
        public RelayCommand FlashPointCommand { get; set; }
        public RelayCommand CopyAllCommand { get; set; }

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

        private string ProcessInput(string input)
        {
            string result = string.Empty;
            ESRI.ArcGIS.Geometry.IPoint point;

            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var coordType = GetCoordinateType(input, out point);

            if (coordType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                amCoordGetter.Point = point;
                result = (point as IConversionNotation).GetDDFromCoords(6);
                UIHelpers.UpdateHistory(input, InputCoordinateHistoryList);
                UpdateInputs();
            }

            return result;
        }

        private void UpdateInputs()
        {
            string coord = string.Empty;
            if(amCoordGetter.CanGetGARS(out coord))
            {
                if (CoordinateGARS.TryParse(coord, out _inputGARS))
                    RaisePropertyChanged(() => InputGARS);
            }

            if(amCoordGetter.CanGetMGRS(out coord))
            {
                if (CoordinateMGRS.TryParse(coord, out _inputMGRS))
                    RaisePropertyChanged(() => InputMGRS);
            }

            if(amCoordGetter.CanGetUSNG(out coord))
            {
                if (CoordinateUSNG.TryParse(coord, out _inputUSNG))
                    RaisePropertyChanged(() => InputUSNG);
            }

            if(amCoordGetter.CanGetUTM(out coord))
            {
                if (CoordinateUTM.TryParse(coord, out _inputUTM))
                {
                    RaisePropertyChanged(() => InputUTM);
                    RaisePropertyChanged(() => InputUTMZoneWithHemi);
                }
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
                dict.Add(CoordinateType.MGRS, cn.CreateMGRS(5, false, esriMGRSModeEnum.esriMGRSMode_NewStyle));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.USNG, cn.GetUSNGFromCoords(5, false,false));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.UTM, cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS));
            }
            catch { }

            Mediator.NotifyColleagues("BROADCAST_COORDINATE_VALUES", dict);
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
