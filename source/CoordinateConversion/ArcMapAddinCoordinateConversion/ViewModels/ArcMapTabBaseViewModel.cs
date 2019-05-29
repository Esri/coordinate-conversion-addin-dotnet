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
using System.Linq;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using ArcMapAddinCoordinateConversion.Helpers;
using System.Globalization;
using ESRI.ArcGIS.Framework;
using System.Windows.Forms;
using ArcMapAddinCoordinateConversion.Models;
using CoordinateConversionLibrary;
using System.Collections.ObjectModel;
using CoordinateConversionLibrary.Views;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using ArcMapAddinCoordinateConversion.ValueConverters;
using System.Windows;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class ArcMapTabBaseViewModel : TabBaseViewModel
    {
        // This name should correlate to the name specified in Config.esriaddinx - Tool id="Esri_ArcMapAddinCoordinateConversion_MapPointTool"
        internal const string MapPointToolName = "Esri_ArcMapAddinCoordinateConversion_MapPointTool";

        public ArcMapTabBaseViewModel()
        {
            // commands
            ActivatePointToolCommand = new RelayCommand(OnActivatePointToolCommand);
            FlashPointCommand = new RelayCommand(OnFlashPointCommand);
            ViewDetailCommand = new RelayCommand(OnViewDetailCommand);
            PreviousRecordCommand = new RelayCommand(OnPreviousRecordCommand);
            NextRecordCommand = new RelayCommand(OnNextRecordCommand);

            FieldsCollection = new ObservableCollection<CoordinateConversionLibrary.ViewModels.FieldsCollection>();
            ViewDetailsTitle = string.Empty;
            IsWarningVisible = Visibility.Collapsed;
            PageNumber = 1;
            IsPreviousRecordEnabled = false;
            IsNextRecordEnabled = CollectTabViewModel.CoordinateAddInPoints != null && CollectTabViewModel.CoordinateAddInPoints.Where(x => x.IsSelected).Count() <= PageNumber;
            Mediator.Register(CoordinateConversionLibrary.Constants.NewMapPointSelection, OnNewMapPointSelection);
            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, amCoordGetter);
        }

        public RelayCommand ActivatePointToolCommand { get; set; }
        public RelayCommand FlashPointCommand { get; set; }
        public RelayCommand ViewDetailCommand { get; set; }
        public RelayCommand PreviousRecordCommand { get; set; }
        public RelayCommand NextRecordCommand { get; set; }
        public CoordinateType InputCoordinateType { get; set; }
        public ICommandItem CurrentTool { get; set; }
        public ObservableCollection<FieldsCollection> FieldsCollection { get; set; }
        public string ViewDetailsTitle { get; set; }
        public AdditionalFieldsView DialogView { get; set; }
        public bool IsDialogViewOpen { get; set; }

        private bool isPreviousRecordEnabled;
        public bool IsPreviousRecordEnabled
        {
            get { return isPreviousRecordEnabled; }
            set
            {
                isPreviousRecordEnabled = value;
                RaisePropertyChanged(() => IsPreviousRecordEnabled);
            }
        }

        private bool isNextRecordEnabled;
        public bool IsNextRecordEnabled
        {
            get { return isNextRecordEnabled; }
            set
            {
                isNextRecordEnabled = value;
                RaisePropertyChanged(() => IsNextRecordEnabled);
            }
        }


        public static ArcMapCoordinateGet amCoordGetter = new ArcMapCoordinateGet();

        private Visibility isWarningVisible;
        public Visibility IsWarningVisible
        {
            get { return isWarningVisible; }
            set
            {
                isWarningVisible = value;
                RaisePropertyChanged(() => IsWarningVisible);
            }
        }

        private int pageNumber;
        public int PageNumber
        {
            get { return pageNumber; }
            set
            {
                pageNumber = value;
                IsPreviousRecordEnabled = (value > 1);
                if (CollectTabViewModel.CoordinateAddInPoints != null)
                    IsNextRecordEnabled = (value < CollectTabViewModel.CoordinateAddInPoints.Where(x => x.IsSelected).Count());
                RaisePropertyChanged(() => PageNumber);
            }
        }


        private void OnNextRecordCommand(object obj)
        {
            PageNumber++;
            var currentPointData = CollectTabViewModel.CoordinateAddInPoints.Where(x => x.IsSelected).ElementAt(PageNumber - 1);
            ShowPopUp(currentPointData);
        }

        private void OnPreviousRecordCommand(object obj)
        {
            PageNumber--;
            var currentPointData = CollectTabViewModel.CoordinateAddInPoints.Where(x => x.IsSelected).ElementAt(PageNumber - 1);
            ShowPopUp(currentPointData);
        }

        internal void OnActivatePointToolCommand(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, MapPointToolName);
        }

        internal virtual void OnFlashPointCommand(object obj)
        {
            ProcessInput(InputCoordinate);
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);


            IGeometry address = obj as IGeometry;

            if (address == null && amCoordGetter != null && amCoordGetter.Point != null)
                address = amCoordGetter.Point;

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

                IMarkerElement markerElement = new MarkerElementClass();

                markerElement.Symbol = simpleMarkerSymbol;

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
                ArcMapHelpers.FlashGeometry(address, color, av.ScreenDisplay, 500, av.Extent);
            }
        }

        internal virtual void OnViewDetailCommand(object obj)
        {
            var input = obj as System.Windows.Controls.ListBox;
            if (input.SelectedItems.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No data available");
                return;
            }
            PageNumber = 1;
            ShowPopUp((CollectTabViewModel.CoordinateAddInPoints.Where(x => x.IsSelected).FirstOrDefault()) as AddInPoint);
        }

        private void ShowPopUp(AddInPoint addinPoint)
        {
            var dictionary = addinPoint.FieldsDictionary;
            FieldsCollection = new ObservableCollection<FieldsCollection>();
            if (dictionary != null)
            {
                var valOutput = dictionary.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault();
                IPointToStringConverter pointConverter = new IPointToStringConverter();
                ViewDetailsTitle = pointConverter.Convert(valOutput, typeof(string), null, null) as string;

                foreach (var item in dictionary)
                {
                    if (item.Value.Item2)
                    {
                        FieldsCollection.Add(new FieldsCollection() { FieldName = item.Key, FieldValue = Convert.ToString(item.Value.Item1) });
                    }
                }
            }
            else
            {
                ViewDetailsTitle = addinPoint.Text;
            }
            IsWarningVisible = FieldsCollection.Any() ? Visibility.Collapsed : Visibility.Visible;
            if (!IsDialogViewOpen)
            {
                IsDialogViewOpen = true;
                PageNumber = 1;
                DialogView = new AdditionalFieldsView();
                DialogView.DataContext = this;
                DialogView.Closed += diagView_Closed;
                DialogView.Show();
            }
            else
            {
                DialogView.DataContext = this;
                RaisePropertyChanged(() => FieldsCollection);
                RaisePropertyChanged(() => ViewDetailsTitle);
            }
        }

        private void diagView_Closed(object sender, EventArgs e)
        {
            IsDialogViewOpen = false;
        }

        public override bool OnNewMapPoint(object obj)
        {
            var input = obj as Dictionary<string, Tuple<object, bool>>;
            IPoint point;
            if (input != null)
                point = input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault() as IPoint;
            else
                point = obj as IPoint;

            amCoordGetter.Point = point;
            InputCoordinate = amCoordGetter.GetInputDisplayString();

            return true;
        }

        public override void OnValidateMapPoint(object obj)
        {
            if (OnValidationSuccess(obj))
            {
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NEW_MAP_POINT, obj);
            }
        }

        public bool OnValidationSuccess(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;
            var input = obj as Dictionary<string, Tuple<object, bool>>;
            IPoint point;
            if (input != null)
                point = input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault() as IPoint;
            else
                point = obj as IPoint;

            if (point == null)
                return false;

            if (!IsValidPoint(point) || InputCoordinate == "NA")
            {
                System.Windows.Forms.MessageBox.Show("Point is out of bounds", "Point is out of bounds", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to check to see point is withing the map area of interest
        /// </summary>
        /// <param name="point">IPoint to validate</param>
        /// <returns></returns>
        internal bool IsValidPoint(IPoint point)
        {
            if ((point != null) && (ArcMap.Document != null) &&
                (ArcMap.Document.ActiveView != null))
            {
                var viewExtent = ArcMap.Document.ActiveView.FullExtent;
                return IsPointWithinExtent(point, viewExtent);
            }
            return false;
        }

        /// <summary>
        /// Method used to check to see if a point is contained by an envelope
        /// </summary>
        /// <param name="point">IPoint</param>
        /// <param name="env">IEnvelope</param>
        /// <returns></returns>
        internal bool IsPointWithinExtent(IPoint point, IEnvelope env)
        {
            var relationOp = env as IRelationalOperator;

            if (relationOp == null)
                return false;

            var envelop = env as IGeometry;
            if (envelop.SpatialReference != point.SpatialReference)
            {
                point.Project(envelop.SpatialReference);
            }
            return relationOp.Contains(point);
        }

        /// <summary>
        /// Unions all extents 
        /// </summary>
        /// <param name="map"></param>
        /// <returns>envelope</returns>
        internal IEnvelope UnionAllLayerExtents(IMap map)
        {
            var layers = map.get_Layers();
            var layer = layers.Next();

            var geomBag = new GeometryBagClass();
            geomBag.SpatialReference = map.SpatialReference;

            var geomColl = (IGeometryCollection)geomBag;
            object MissingType = Type.Missing;

            while (layer != null)
            {
                if (layer.AreaOfInterest != null)
                {
                    geomColl.AddGeometry(layer.AreaOfInterest, ref MissingType, ref MissingType);
                }
                layer = layers.Next();
            }

            return geomBag.Envelope;
        }

        public override bool OnMouseMove(object obj)
        {
            if (!base.OnMouseMove(obj))
                return false;

            var point = obj as IPoint;

            if (point == null)
                return false;

            amCoordGetter.Point = point;
            InputCoordinate = amCoordGetter.GetInputDisplayString();

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            if (amCoordGetter != null && amCoordGetter.Point != null)
            {
                InputCoordinate = amCoordGetter.GetInputDisplayString();
            }
        }

        public override string ProcessInput(string input)
        {
            if (input == "NA") return string.Empty;

            base.ProcessInput(input);

            string result = string.Empty;
            ESRI.ArcGIS.Geometry.IPoint point;

            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            InputCoordinateType = GetCoordinateType(input, out point);

            if (InputCoordinateType == CoordinateType.Unknown)
            {
                HasInputError = true;
                amCoordGetter.Point = null;
                foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    output.OutputCoordinate = "";
                    output.Props.Clear();
                }
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.InvalidCoordMsg,
                    CoordinateConversionLibrary.Properties.Resources.InvalidCoordCap);
            }
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
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                if (CoordinateBase.InputFormatSelection == CoordinateTypes.Custom.ToString())
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                else
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
            }

            return result;
        }

        public override bool CheckMapLoaded()
        {
            return ArcMap.LayerCount > 0;
        }

        public Dictionary<string, string> GetOutputFormats(AddInPoint input)
        {
            var results = new Dictionary<string, string>();
            IPoint point;
            var inputText = input.Point.Y + " " + input.Point.X;
            var ctype = GetCoordinateType(inputText, out point);
            if (point != null)
            {
                ArcMapCoordinateGet arcMapCoordinateGetter = new ArcMapCoordinateGet();
                arcMapCoordinateGetter.Point = point;
                CoordinateGetBase coordinateGetter = arcMapCoordinateGetter as CoordinateGetBase;
                results.Add(CoordinateFieldName, input.Text);
                foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    var props = new Dictionary<string, string>();
                    string coord = string.Empty;

                    switch (output.CType)
                    {
                        case CoordinateType.DD:
                            CoordinateDD cdd;
                            if (coordinateGetter.CanGetDD(output.SRFactoryCode, out coord) &&
                                CoordinateDD.TryParse(coord, out cdd, true))
                            {
                                results.Add(output.Name, cdd.ToString(output.Format, new CoordinateDDFormatter()));
                            }
                            break;
                        case CoordinateType.DMS:
                            CoordinateDMS cdms;
                            if (coordinateGetter.CanGetDMS(output.SRFactoryCode, out coord) &&
                                CoordinateDMS.TryParse(coord, out cdms, true))
                            {
                                results.Add(output.Name, cdms.ToString(output.Format, new CoordinateDMSFormatter()));
                            }
                            break;
                        case CoordinateType.DDM:
                            CoordinateDDM ddm;
                            if (coordinateGetter.CanGetDDM(output.SRFactoryCode, out coord) &&
                                CoordinateDDM.TryParse(coord, out ddm, true))
                            {
                                results.Add(output.Name, ddm.ToString(output.Format, new CoordinateDDMFormatter()));
                            }
                            break;
                        case CoordinateType.GARS:
                            CoordinateGARS gars;
                            if (coordinateGetter.CanGetGARS(output.SRFactoryCode, out coord) &&
                                CoordinateGARS.TryParse(coord, out gars))
                            {
                                results.Add(output.Name, gars.ToString(output.Format, new CoordinateGARSFormatter()));
                            }
                            break;
                        case CoordinateType.MGRS:
                            CoordinateMGRS mgrs;
                            if (coordinateGetter.CanGetMGRS(output.SRFactoryCode, out coord) &&
                                CoordinateMGRS.TryParse(coord, out mgrs))
                            {
                                results.Add(output.Name, mgrs.ToString(output.Format, new CoordinateMGRSFormatter()));
                            }
                            break;
                        case CoordinateType.USNG:
                            CoordinateUSNG usng;
                            if (coordinateGetter.CanGetUSNG(output.SRFactoryCode, out coord) &&
                                CoordinateUSNG.TryParse(coord, out usng))
                            {
                                results.Add(output.Name, usng.ToString(output.Format, new CoordinateMGRSFormatter()));
                            }
                            break;
                        case CoordinateType.UTM:
                            CoordinateUTM utm;
                            if (coordinateGetter.CanGetUTM(output.SRFactoryCode, out coord) &&
                                CoordinateUTM.TryParse(coord, out utm))
                            {
                                results.Add(output.Name, utm.ToString(output.Format, new CoordinateUTMFormatter()));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return results;
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

        public void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
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
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.DDM, cn.GetDDMFromCoords(6));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.DMS, cn.GetDMSFromCoords(6));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.GARS, cn.GetGARSFromCoords());
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.MGRS, cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.USNG, cn.GetUSNGFromCoords(5, true, false));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.UTM, cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces | esriUTMConversionOptionsEnum.esriUTMUseNS));
            }
            catch { /* Conversion Failed */ }

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);
        }

        internal IPolygon GetMGRSPolygon(IPoint point)
        {
            CoordinateMGRS mgrs;

            IPointCollection pc = new RingClass();

            // bottom left
            CoordinateMGRS.TryParse(InputCoordinate, out mgrs);

            if (mgrs == null)
                return null;

            // don't create a polygon for 1m resolution
            if (mgrs.Easting.ToString().Length > 4 && mgrs.Northing.ToString().Length > 4)
                return null;

            var tempPoint = (IConversionNotation)new PointClass();
            ((IPoint)tempPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            var anotherMGRSstring = mgrs.ToString("", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // top left
            var tempMGRS = new CoordinateMGRS(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
            var tempEasting = mgrs.Easting.ToString().PadRight(5, '0');
            tempMGRS.Easting = Convert.ToInt32(tempEasting);
            var tempNorthing = mgrs.Northing.ToString().PadRight(5, '9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

            tempPoint = (IConversionNotation)new PointClass();
            ((IPoint)tempPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            anotherMGRSstring = tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
            tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // top right
            tempEasting = mgrs.Easting.ToString().PadRight(5, '9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5, '9');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

            tempPoint = (IConversionNotation)new PointClass();
            ((IPoint)tempPoint).SpatialReference = ArcMapHelpers.GetGCS_WGS_1984_SR();
            tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
            pc.AddPoint(tempPoint as IPoint);

            // bottom right
            tempEasting = mgrs.Easting.ToString().PadRight(5, '9');
            tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
            tempNorthing = mgrs.Northing.ToString().PadRight(5, '0');
            tempMGRS.Northing = Convert.ToInt32(tempNorthing);

            tempPoint = (IConversionNotation)new PointClass();
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

        private CoordinateType GetCoordinateType(string input, out ESRI.ArcGIS.Geometry.IPoint point)
        {
            point = new PointClass();
            var cn = (IConversionNotation)point;
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            if (t == null)
                return CoordinateType.Unknown;

            System.Object obj = Activator.CreateInstance(t);
            if (obj == null)
                return CoordinateType.Unknown;

            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;
            if (srFact == null)
                return CoordinateType.Unknown;

            // Use the enumeration to create an instance of the predefined object.
            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            point.SpatialReference = geographicCS;
            string numSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            input = numSep != "." ? input.Replace(numSep, ".") : input;

            try
            {
                CoordinateDD dd;
                if (CoordinateDD.TryParse(input, out dd, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1}", dd.Lat, dd.Lon);
                    cn.PutCoordsFromDD(newInput);

                    return CoordinateType.DD;
                }
            }
            catch { /* Conversion Failed */ }

            try
            {
                CoordinateDDM ddm;
                if (CoordinateDDM.TryParse(input, out ddm, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", ddm.LatDegrees, ddm.LatMinutes, ddm.LonDegrees, ddm.LonMinutes);
                    cn.PutCoordsFromDD(newInput);

                    return CoordinateType.DDM;
                }
            }
            catch { /* Conversion Failed */ }

            try
            {
                CoordinateDMS dms;
                if (CoordinateDMS.TryParse(input, out dms, true))
                {
                    // Reformat the string for cases where lat/lon have been switched
                    // PutCoords calls fail if the double uses decimal separator other than a decimal point
                    // Added InvariantCulture option to ensure the current culture is ignored
                    string newInput = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5}", dms.LatDegrees, dms.LatMinutes, dms.LatSeconds, dms.LonDegrees, dms.LonMinutes, dms.LonSeconds);
                    cn.PutCoordsFromDD(newInput);

                    return CoordinateType.DMS;
                }
            }
            catch { /* Conversion Failed */ }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, input);
                return CoordinateType.GARS;
            }
            catch { /* Conversion Failed */ }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeLL, input);
                return CoordinateType.GARS;
            }
            catch { /* Conversion Failed */ }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, gars.ToString("", new CoordinateGARSFormatter()));
                    return CoordinateType.GARS;
                }
                catch { /* Conversion Failed */ }
            }

            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
                return CoordinateType.MGRS;
            }
            catch { /* Conversion Failed */ }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewStyle);
                return CoordinateType.MGRS;
            }
            catch { /* Conversion Failed */ }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewWith180InZone01);
                return CoordinateType.MGRS;
            }
            catch { /* Conversion Failed */ }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldStyle);
                return CoordinateType.MGRS;
            }
            catch { /* Conversion Failed */ }
            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldWith180InZone01);
                return CoordinateType.MGRS;
            }
            catch { /* Conversion Failed */ }

            // mgrs try parse
            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    cn.PutCoordsFromMGRS(mgrs.ToString("", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_NewStyle);
                    return CoordinateType.MGRS;
                }
                catch { /* Conversion Failed */ }
            }

            try
            {
                cn.PutCoordsFromUSNG(input);
                return CoordinateType.USNG;
            }
            catch { /* Conversion Failed */ }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    cn.PutCoordsFromUSNG(usng.ToString("", new CoordinateMGRSFormatter()));
                    return CoordinateType.USNG;
                }
                catch { /* Conversion Failed */ }
            }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMAddSpaces, input);
                return CoordinateType.UTM;
            }
            catch { /* Conversion Failed */ }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, input);
                return CoordinateType.UTM;
            }
            catch { /* Conversion Failed */ }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMUseNS, input);
                return CoordinateType.UTM;
            }
            catch { /* Conversion Failed */ }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, utm.ToString("", new CoordinateUTMFormatter()));
                    return CoordinateType.UTM;
                }
                catch { /* Conversion Failed */ }
            }

            /*
             * Updated RegEx to capture invalid coordinates like 00, 45, or 456987. 
             */
            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]{1,}(?<longitude>\-?\d+[.,]?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    IMap map = ((IMxDocument)ArcMap.Application.Document).FocusMap;
                    var sr = map.SpatialReference != null ? map.SpatialReference : ArcMapHelpers.GetSR((int)esriSRProjCS3Type.esriSRProjCS_WGS1984WebMercatorMajorAuxSphere);
                    point.X = Lon;
                    point.Y = Lat;
                    point.SpatialReference = sr;
                    return CoordinateType.DD;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return CoordinateType.Unknown;
        }

        public override List<Dictionary<string, Tuple<object, bool>>> ReadExcelInput(string fileName)
        {
            var tableName = Guid.NewGuid().ToString().Replace("-", "");
            var lstDictionary = new List<Dictionary<string, Tuple<object, bool>>>();
            using (ComReleaser oComReleaser = new ComReleaser())
            {

                IFeatureWorkspace workspace = CreateFeatureWorkspace("tempWorkspace");
                if (workspace == null)
                {
                }
                // Cast the workspace to the IWorkspaceEdit interface.
                IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

                // Start an edit session. An undo/redo stack isn't necessary in this case.
                workspaceEdit.StartEditing(false);

                // Start an edit operation.
                workspaceEdit.StartEditOperation();

                IGeoProcessor2 gp = new GeoProcessorClass();
                gp.AddOutputsToMap = false;

                IVariantArray rasterToPolyParams = new VarArrayClass();
                rasterToPolyParams.Add(fileName);
                rasterToPolyParams.Add(((IWorkspace)workspace).PathName + System.IO.Path.DirectorySeparatorChar + tableName);
                object oResult = gp.Execute("ExcelToTable_conversion", rasterToPolyParams, null);
                IGeoProcessorResult ipResult = (IGeoProcessorResult)oResult;

                // Save the edit operation. To cancel an edit operation, the AbortEditOperation
                // method can be used.
                workspaceEdit.StopEditOperation();

                // Stop the edit session. The saveEdits parameter indioates the edit session
                // will be committed.
                workspaceEdit.StopEditing(true);

                if (ipResult.Status == esriJobStatus.esriJobSucceeded)
                {
                    var workspacePath = System.IO.Path.GetDirectoryName(Convert.ToString(ipResult.ReturnValue));

                    var workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(typeof(FileGDBWorkspaceFactoryClass));
                    var featureWorkspace = workspaceFactory.OpenFromFile(workspacePath, 0);
                    ITable table = GetTables(featureWorkspace, tableName);
                    if (table != null)
                    {
                        for (int i = 1; i <= table.RowCount(null); i++)
                        {
                            var dictionary = new Dictionary<string, Tuple<object, bool>>();
                            IRow row = table.GetRow(i);
                            for (int j = 0; j < row.Fields.FieldCount; j++)
                            {
                                dictionary.Add(row.Fields.get_Field(j).Name, Tuple.Create(row.get_Value(j), false));
                            }
                            lstDictionary.Add(dictionary);
                        }
                    }
                    var ds = table as IDataset;
                    ds.Delete();
                }
            }
            return lstDictionary;
        }

        static ITable GetTables(IWorkspace workspace, string tableName)
        {
            var enumDataset = workspace.Datasets[esriDatasetType.esriDTTable];
            ITable dataTable = null;
            IDataset dataset;
            while ((dataset = enumDataset.Next()) != null)
            {
                var dSet = dataset as ITable;
                if ((dSet as IDataset).BrowseName == tableName)
                    dataTable = dSet;
            }
            return dataTable;
        }

        public static IWorkspace OpenWorkspace(string workspacePath)
        {
            IWorkspaceFactory workspaceFactory = new AccessWorkspaceFactoryClass();
            return workspaceFactory.OpenFromFile(workspacePath, 0);
        }

        public static IFeatureWorkspace CreateFeatureWorkspace(string workspaceNameString)
        {

            IScratchWorkspaceFactory2 ipScWsFactory = new FileGDBScratchWorkspaceFactoryClass();
            IWorkspace ipScWorkspace = ipScWsFactory.CurrentScratchWorkspace;
            if (null == ipScWorkspace)
                ipScWorkspace = ipScWsFactory.CreateNewScratchWorkspace();

            IFeatureWorkspace featWork = (IFeatureWorkspace)ipScWorkspace;

            return featWork;
        }

        /// <summary>
        /// Start Editing operation
        /// </summary>
        /// <param name="ipWorkspace">IWorkspace</param>
        public static bool StartEditOperation(IWorkspace ipWorkspace)
        {
            bool blnWasSuccessful = false;
            IWorkspaceEdit ipWsEdit = ipWorkspace as IWorkspaceEdit;

            if (ipWsEdit != null)
            {
                try
                {
                    ipWsEdit.StartEditOperation();
                    blnWasSuccessful = true;
                }
                catch (Exception ex)
                {
                    ipWsEdit.AbortEditOperation();
                    throw (ex);
                }
            }

            return blnWasSuccessful;
        }

        /// <summary>
        /// Stop Editing operation
        /// </summary>
        /// <param name="ipWorkspace">IWorkspace</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool StopEditOperation(IWorkspace ipWorkspace)
        {
            bool blnWasSuccessful = false;
            IWorkspaceEdit ipWsEdit = ipWorkspace as IWorkspaceEdit;
            if (ipWsEdit != null)
            {
                try
                {
                    ipWsEdit.StopEditOperation();
                    blnWasSuccessful = true;
                }
                catch (Exception ex)
                {
                    ipWsEdit.AbortEditOperation();
                    throw (ex);
                }
            }

            return blnWasSuccessful;
        }
    }
}