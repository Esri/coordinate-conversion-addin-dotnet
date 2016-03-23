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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CoordinateToolLibrary.Views;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Helpers;
using ArcGIS.Core.Geometry;
using CoordinateToolLibrary.ViewModels;
using System.ComponentModel;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Core.Data;

namespace ProAppCoordToolModule
{
    internal class CoordinateToolDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ProAppCoordToolModule_CoordinateToolDockpane";

        protected CoordinateToolDockpaneViewModel() 
        {
            _coordinateToolView = new CoordinateToolView();
            HasInputError = false;
            IsHistoryUpdate = false;
            AddNewOCCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnAddNewOCCommand);
            ActivatePointToolCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnFlashPointCommand);
            CopyAllCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnCopyAllCommand);
            Mediator.Register(CoordinateToolLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            InputCoordinateHistoryList = new ObservableCollection<string>();
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);

            var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
            if (ctvm != null)
            {
                ctvm.SetCoordinateGetter(proCoordGetter);
            }
        }

        ~CoordinateToolDockpaneViewModel()
        {
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
        }

        public bool IsToolActive
        {
            get
            {
                if (FrameworkApplication.CurrentTool != null)
                    return FrameworkApplication.CurrentTool == "ProAppCoordToolModule_CoordinateMapTool";

                return false;
            }
            set
            {
                if (value)
                    OnMapToolCommand(null);
                else
                    FrameworkApplication.SetCurrentToolAsync(string.Empty);

                NotifyPropertyChanged(new PropertyChangedEventArgs("IsToolActive"));
            }
        }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        private void BroadcastCoordinateValues(MapPoint mapPoint)
        {
            var dict = new Dictionary<CoordinateType, string>();
            if (mapPoint == null)
                return;

            var dd = new CoordinateDD(mapPoint.Y, mapPoint.X);

            try
            {
                dict.Add(CoordinateType.DD, dd.ToString("", new CoordinateDDFormatter()));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.DDM, new CoordinateDDM(dd).ToString("", new CoordinateDDMFormatter()));
            }
            catch { }
            try
            {
                dict.Add(CoordinateType.DMS, new CoordinateDMS(dd).ToString("", new CoordinateDMSFormatter()));
            }
            catch { }

            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.BroadcastCoordinateValues, dict);

        }
        private static System.IDisposable _overlayObject = null;

        private void ClearOverlay()
        {
            if (_overlayObject != null)
            {
                _overlayObject.Dispose();
                _overlayObject = null;
            }
        }

        private ProCoordinateGet proCoordGetter = new ProCoordinateGet();

        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("HasInputError"));
            }
        }

        public CoordinateToolLibrary.Helpers.RelayCommand AddNewOCCommand { get; set; }
        public CoordinateToolLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateToolLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }
        public CoordinateToolLibrary.Helpers.RelayCommand CopyAllCommand { get; set; }

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
                ClearOverlay();

                if (string.IsNullOrWhiteSpace(value))
                    return;

                _inputCoordinate = value;
                var tempDD = ProcessInput(_inputCoordinate);

                // update tool view model
                var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
                if (ctvm != null)
                {
                    ctvm.SetCoordinateGetter(proCoordGetter);
                    ctvm.InputCoordinate = tempDD;
                }

                NotifyPropertyChanged(new PropertyChangedEventArgs("InputCoordinate"));
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

        #region command handlers

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            string name = CoordinateType.DD.ToString();
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.AddNewOutputCoordinate, new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#N X0.0#E" });
        }

        private void OnMapToolCommand(object obj)
        {
            FrameworkApplication.SetCurrentToolAsync("ProAppCoordToolModule_CoordinateMapTool");
        }

        private async void OnFlashPointCommand(object obj)
        {
            var previous = IsToolActive;
            if (!IsToolActive)
            {
                IsToolActive = true;
            }

            CoordinateDD dd;
            var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
            if (ctvm != null)
            {
                if (!CoordinateDD.TryParse(ctvm.InputCoordinate, out dd))
                    return;
            }
            else { return; }

            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;
            var point = await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
            });

            //await QueuedTask.Run(() =>
            //{
            //    // Construct point symbol
            //    symbol = SymbolFactory.ConstructPointSymbol(ColorFactory.Red, 10.0, SimpleMarkerStyle.Star);
            //});

            ////Get symbol reference from the symbol 
            //CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            //QueuedTask.Run(() =>
            //{
            //    ClearOverlay();
            //    _overlayObject = MapView.Active.AddOverlay(point, symbolReference);
            //    //MapView.Active.ZoomToAsync(point, new TimeSpan(2500000), true);
            //});


            QueuedTask.Run(() =>
                {
                    Mediator.NotifyColleagues("UPDATE_FLASH", point);
                });

            //await QueuedTask.Run(() =>
            //{
            //    Task.Delay(500);
            //    ClearOverlay();
            //    //_overlayObject = MapView.Active.AddOverlay(point, symbolReference);
            //    //MapView.Active.ZoomToAsync(point, new TimeSpan(2500000), true);
            //});
            //if (previous != IsToolActive)
            //    IsToolActive = previous;
            //await QueuedTask.Run(() =>
            //{
            //    Task.Delay(500);
            //    ClearOverlay();
            //    MapView.Active.LookAt(MapView.Active.Extent.Center);
            //});
        }

        private void OnCopyAllCommand(object obj)
        {
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.CopyAllCoordinateOutputs, InputCoordinate);
        }

        private void OnBCNeeded(object obj)
        {
            if (proCoordGetter == null || proCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(proCoordGetter.Point);
        }

        #endregion command handlers

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
            MapPoint point;
            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var coordType = GetCoordinateType(input, out point);

            if (coordType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                proCoordGetter.Point = point;
                result = new CoordinateDD(point.Y, point.X).ToString("", new CoordinateDDFormatter());
                if (IsHistoryUpdate)
                {
                    UIHelpers.UpdateHistory(input, InputCoordinateHistoryList);
                    IsHistoryUpdate = false;
                }
            }

            return result;
        }

        private CoordinateType GetCoordinateType(string input, out MapPoint point)
        {
            point = null;

            // DD
            CoordinateDD dd;
            if(CoordinateDD.TryParse(input, out dd))
            {
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DD;
            }

            // DDM
            CoordinateDDM ddm;
            if(CoordinateDDM.TryParse(input, out ddm))
            {
                dd = new CoordinateDD(ddm);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DDM;
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms))
            {
                dd = new CoordinateDD(dms);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DMS;
            }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(gars.ToString("", new CoordinateGARSFormatter()), sptlRef, GeoCoordinateType.GARS, FromGeoCoordinateMode.Default);
                        return tmp;
                    }).Result;
                    
                    return CoordinateType.GARS;
                }
                catch { }
            }

            CoordinateMGRS mgrs;
            if(CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(mgrs.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.MGRS, FromGeoCoordinateMode.Default);
                        return tmp;
                    }).Result;

                    return CoordinateType.MGRS;
                }
                catch { }
            }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(usng.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.USNG, FromGeoCoordinateMode.Default);
                        return tmp;
                    }).Result;

                    return CoordinateType.USNG;
                }
                catch { }
            }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(utm.ToString("", new CoordinateUTMFormatter()), sptlRef, GeoCoordinateType.UTM, FromGeoCoordinateMode.Default);
                        return tmp;
                    }).Result;

                    return CoordinateType.UTM;
                }
                catch { }
            }


            return CoordinateType.Unknown;
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        private object _lock = new object();
        private async void OnSelectionChanged(MapSelectionChangedEventArgs obj)
        {
            if (MapView.Active.Map != null && obj.Selection.Count == 1)
            {
                var fl = obj.Selection.FirstOrDefault().Key as FeatureLayer;
                if (fl == null || fl.SelectionCount != 1 || fl.ShapeType != esriGeometryType.esriGeometryPoint)
                    return;

                var pointd = await QueuedTask.Run(() =>
                {
                    try
                    {
                        var SelectedOID = fl.GetSelection().GetObjectIDs().FirstOrDefault();
                        if (SelectedOID < 0)
                            return string.Empty;

                        var SelectedLayer = fl as BasicFeatureLayer;

                        var oidField = SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                        var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, SelectedOID) };
                        var cursor = SelectedLayer.Search(qf);
                        Row row = null;

                        if (cursor.MoveNext())
                            row = cursor.Current;

                        if (row == null)
                            return string.Empty;

                        var fields = row.GetFields();
                        lock (_lock)
                        {
                            foreach (ArcGIS.Core.Data.Field field in fields)
                            {
                                if (field.FieldType == FieldType.Geometry)
                                {
                                    // have mappoint here
                                    var val = row[field.Name];
                                    if (val is MapPoint)
                                    {
                                        var temp = val as MapPoint;
                                        // project to WGS1984
                                        proCoordGetter.Point = temp;
                                        proCoordGetter.Project(4326);
                                        return string.Format("{0:0.0####} {1:0.0####}", proCoordGetter.Point.Y, proCoordGetter.Point.X);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    return string.Empty;
                });

                if (!string.IsNullOrWhiteSpace(pointd))
                {
                    InputCoordinate = pointd;
                }
            }
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CoordinateToolDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CoordinateToolDockpaneViewModel.Show();
        }
    }
}
