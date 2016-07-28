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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Helpers;
using ArcGIS.Core.Geometry;
using CoordinateConversionLibrary.ViewModels;
using System.ComponentModel;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Core.Data;
using System.Text.RegularExpressions;
using ProAppCoordConversionModule.ViewModels;
using System.Windows.Controls;
using CoordinateConversionLibrary;

namespace ProAppCoordConversionModule
{
    internal class CoordinateConversionDockpaneViewModel : DockPane
    {
        protected CoordinateConversionDockpaneViewModel() 
        {
            ConvertTabView = new CCConvertTabView();
            ConvertTabView.DataContext = new ProConvertTabViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new ProCollectTabViewModel();

            //_coordinateConversionView = new CCConvertTabView();
            //HasInputError = false;
            //IsHistoryUpdate = true;
            //IsToolGenerated = false;
            //ActivatePointToolCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnMapToolCommand);
            //FlashPointCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnFlashPointCommand);
            //EditPropertiesDialogCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnEditPropertiesDialogCommand);
            //Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            //InputCoordinateHistoryList = new ObservableCollection<string>();
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);

            //Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, proCoordGetter);

            //configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
            //.RegisterHandler(n => n.DisplayCoordinateType, n => {
            //    if(proCoordGetter != null && proCoordGetter.Point != null)
            //    {
            //        InputCoordinate = proCoordGetter.GetInputDisplayString();
            //    }
            //});
        }

        ~CoordinateConversionDockpaneViewModel()
        {
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
        }

        private const string _dockPaneID = "ProAppCoordConversionModule_CoordinateConversionDockpane";
        
        public CCConvertTabView ConvertTabView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        object selectedTab = null;
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value)
                    return;

                selectedTab = value;
                var tabItem = selectedTab as TabItem;
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
                //TODO let the other viewmodels determine what to do when tab selection changes
                if (tabItem.Header.ToString() == CoordinateConversionLibrary.Properties.Resources.HeaderCollect)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Collect);
                else
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Convert);
            }
        }

        //PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        //public bool IsToolActive
        //{
        //    get
        //    {
        //        if (FrameworkApplication.CurrentTool != null)
        //            return FrameworkApplication.CurrentTool == "ProAppCoordConversionModule_CoordinateMapTool";

        //        return false;
        //    }
        //    set
        //    {
        //        if (value)
        //            OnMapToolCommand(null);
        //        else
        //            FrameworkApplication.SetCurrentToolAsync(string.Empty);

        //        NotifyPropertyChanged(new PropertyChangedEventArgs("IsToolActive"));
        //    }
        //}

        //public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        //private void BroadcastCoordinateValues(MapPoint mapPoint)
        //{
        //    var dict = new Dictionary<CoordinateType, string>();
        //    if (mapPoint == null)
        //        return;

        //    var dd = new CoordinateDD(mapPoint.Y, mapPoint.X);

        //    try
        //    {
        //        dict.Add(CoordinateType.DD, dd.ToString("", new CoordinateDDFormatter()));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.DDM, new CoordinateDDM(dd).ToString("", new CoordinateDDMFormatter()));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.DMS, new CoordinateDMS(dd).ToString("", new CoordinateDMSFormatter()));
        //    }
        //    catch { }

        //    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);

        //}
        //private static System.IDisposable _overlayObject = null;

        //private void ClearOverlay()
        //{
        //    if (_overlayObject != null)
        //    {
        //        _overlayObject.Dispose();
        //        _overlayObject = null;
        //    }
        //}

        //private ProCoordinateGet proCoordGetter = new ProCoordinateGet();

        //private bool _hasInputError = false;
        //public bool HasInputError
        //{
        //    get { return _hasInputError; }
        //    set
        //    {
        //        _hasInputError = value;
        //        NotifyPropertyChanged(new PropertyChangedEventArgs("HasInputError"));
        //    }
        //}

        //public CoordinateConversionLibrary.Helpers.RelayCommand AddNewOCCommand { get; set; }
        //public CoordinateConversionLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        //public CoordinateConversionLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }
        //public CoordinateConversionLibrary.Helpers.RelayCommand CopyAllCommand { get; set; }
        //public CoordinateConversionLibrary.Helpers.RelayCommand EditPropertiesDialogCommand { get; set; }

        //public bool IsHistoryUpdate { get; set; }
        //public bool IsToolGenerated { get; set; }

        //private string _inputCoordinate;
        //public string InputCoordinate
        //{
        //    get
        //    {
        //        return _inputCoordinate;
        //    }

        //    set
        //    {
        //        ClearOverlay();

        //        if (string.IsNullOrWhiteSpace(value))
        //            return;

        //        _inputCoordinate = value;
        //        var tempDD = ProcessInput(_inputCoordinate);

        //        // update tool view model
        //        //TODO update for refactor, use Mediator, etc
        //        //var ctvm = CTView.Resources["CTViewModel"] as CoordinateConversionViewModel;
        //        //if (ctvm != null)
        //        //{
        //        //    ctvm.SetCoordinateGetter(proCoordGetter);
        //        //    ctvm.InputCoordinate = tempDD;
        //        //    var formattedInputCoordinate = proCoordGetter.GetInputDisplayString();
        //        //    // update history
        //        //    if (IsHistoryUpdate)
        //        //    {
        //        //        if (IsToolGenerated)
        //        //            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);
        //        //        else
        //        //            UIHelpers.UpdateHistory(_inputCoordinate, InputCoordinateHistoryList);
        //        //    }
        //        //    // reset flags
        //        //    IsHistoryUpdate = true;
        //        //    IsToolGenerated = false;

        //        //    _inputCoordinate = formattedInputCoordinate;
        //        //}

        //        NotifyPropertyChanged(new PropertyChangedEventArgs("InputCoordinate"));
        //    }
        //}

        //private CCConvertTabView _coordinateConversionView;
        //public CCConvertTabView CTView
        //{
        //    get
        //    {
        //        return _coordinateConversionView;
        //    }
        //    set
        //    {
        //        _coordinateConversionView = value;
        //    }
        //}

        #region command handlers


        //private void OnMapToolCommand(object obj)
        //{
        //    FrameworkApplication.SetCurrentToolAsync("ProAppCoordConversionModule_CoordinateMapTool");
        //}

        //private async void OnFlashPointCommand(object obj)
        //{
        //    MapPoint point = null;
        //    var previous = IsToolActive;
        //    if (!IsToolActive)
        //    {
        //        IsToolActive = true;
        //    }

        //    //TODO fix this
        //    //CoordinateDD dd;
        //    //var ctvm = CTView.Resources["CTViewModel"] as CoordinateConversionViewModel;
        //    //if (ctvm != null)
        //    //{
        //    //    if (!CoordinateDD.TryParse(ctvm.InputCoordinate, out dd))
        //    //    {
        //    //        Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

        //    //        var matchMercator = regexMercator.Match(ctvm.InputCoordinate);

        //    //        if (matchMercator.Success && matchMercator.Length == ctvm.InputCoordinate.Length)
        //    //        {
        //    //            try
        //    //            {
        //    //                var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
        //    //                var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
        //    //                point = QueuedTask.Run(() =>
        //    //                {
        //    //                    return MapPointBuilder.CreateMapPoint(Lon, Lat, SpatialReferences.WebMercator);
        //    //                }).Result;
        //    //            }
        //    //            catch (Exception ex)
        //    //            {
        //    //                // do nothing
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            return;
        //    //        }
        //    //    }
        //    //}
        //    //else { return; }

        //    ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

        //    //TODO fix this also
        //    //if (point == null)
        //    //{
        //    //    point = await QueuedTask.Run(() =>
        //    //    {
        //    //        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //    //        return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
        //    //    });
        //    //}

        //    ////

        //    //await QueuedTask.Run(() =>
        //    //{
        //    //    // Construct point symbol
        //    //    symbol = SymbolFactory.ConstructPointSymbol(ColorFactory.Red, 10.0, SimpleMarkerStyle.Star);
        //    //});

        //    ////Get symbol reference from the symbol 
        //    //CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

        //    //QueuedTask.Run(() =>
        //    //{
        //    //    ClearOverlay();
        //    //    _overlayObject = MapView.Active.AddOverlay(point, symbolReference);
        //    //    //MapView.Active.ZoomToAsync(point, new TimeSpan(2500000), true);
        //    //});


        //    await QueuedTask.Run(() =>
        //        {
        //            // is point within current map extent
        //            var projectedPoint = GeometryEngine.Project(point, MapView.Active.Extent.SpatialReference);
        //            if(!GeometryEngine.Contains(MapView.Active.Extent, projectedPoint))
        //            {
        //                MapView.Active.PanTo(point);
        //            }
        //            Mediator.NotifyColleagues("UPDATE_FLASH", point);
        //        });

        //    //await QueuedTask.Run(() =>
        //    //{
        //    //    Task.Delay(500);
        //    //    ClearOverlay();
        //    //    //_overlayObject = MapView.Active.AddOverlay(point, symbolReference);
        //    //    //MapView.Active.ZoomToAsync(point, new TimeSpan(2500000), true);
        //    //});
        //    //if (previous != IsToolActive)
        //    //    IsToolActive = previous;
        //    //await QueuedTask.Run(() =>
        //    //{
        //    //    Task.Delay(500);
        //    //    ClearOverlay();
        //    //    MapView.Active.LookAt(MapView.Active.Extent.Center);
        //    //});
        //}

        //private void OnEditPropertiesDialogCommand(object obj)
        //{
        //    var dlg = new EditPropertiesView();

        //    dlg.ShowDialog();
        //}

        //private void OnBCNeeded(object obj)
        //{
        //    if (proCoordGetter == null || proCoordGetter.Point == null)
        //        return;

        //    BroadcastCoordinateValues(proCoordGetter.Point);
        //}

        #endregion command handlers

        //internal string GetFormattedCoordinate(string coord, CoordinateType cType)
        //{
        //    string format = "";

        //    var tt = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.FirstOrDefault(t => t.CType == cType);
        //    if (tt != null)
        //    {
        //        format = tt.Format;
        //        Console.WriteLine(tt.Format);
        //    }

        //    var cf = CoordinateHandler.GetFormattedCoord(cType, coord, format);

        //    if (!String.IsNullOrWhiteSpace(cf))
        //        return cf;

        //    return string.Empty;
        //}

        //private string ProcessInput(string input)
        //{
        //    string result = string.Empty;
        //    MapPoint point;
        //    HasInputError = false;

        //    if (string.IsNullOrWhiteSpace(input))
        //        return result;

        //    var coordType = GetCoordinateType(input, out point);

        //    if (coordType == CoordinateType.Unknown)
        //        HasInputError = true;
        //    else
        //    {
        //        proCoordGetter.Point = point;
        //        result = new CoordinateDD(point.Y, point.X).ToString("", new CoordinateDDFormatter());
        //    }

        //    return result;
        //}

        //private CoordinateType GetCoordinateType(string input, out MapPoint point)
        //{
        //    point = null;

        //    // DD
        //    CoordinateDD dd;
        //    if(CoordinateDD.TryParse(input, out dd))
        //    {
        //        point = QueuedTask.Run(() =>
        //        {
        //            ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //            return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
        //        }).Result;
        //        return CoordinateType.DD;
        //    }

        //    // DDM
        //    CoordinateDDM ddm;
        //    if(CoordinateDDM.TryParse(input, out ddm))
        //    {
        //        dd = new CoordinateDD(ddm);
        //        point = QueuedTask.Run(() =>
        //        {
        //            ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //            return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
        //        }).Result;
        //        return CoordinateType.DDM;
        //    }
        //    // DMS
        //    CoordinateDMS dms;
        //    if (CoordinateDMS.TryParse(input, out dms))
        //    {
        //        dd = new CoordinateDD(dms);
        //        point = QueuedTask.Run(() =>
        //        {
        //            ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //            return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
        //        }).Result;
        //        return CoordinateType.DMS;
        //    }

        //    CoordinateGARS gars;
        //    if (CoordinateGARS.TryParse(input, out gars))
        //    {
        //        try
        //        {
        //            point = QueuedTask.Run(() =>
        //            {
        //                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //                var tmp = MapPointBuilder.FromGeoCoordinateString(gars.ToString("", new CoordinateGARSFormatter()), sptlRef, GeoCoordinateType.GARS, FromGeoCoordinateMode.Default);
        //                return tmp;
        //            }).Result;
                    
        //            return CoordinateType.GARS;
        //        }
        //        catch { }
        //    }

        //    CoordinateMGRS mgrs;
        //    if(CoordinateMGRS.TryParse(input, out mgrs))
        //    {
        //        try
        //        {
        //            point = QueuedTask.Run(() =>
        //            {
        //                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //                var tmp = MapPointBuilder.FromGeoCoordinateString(mgrs.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.MGRS, FromGeoCoordinateMode.Default);
        //                return tmp;
        //            }).Result;

        //            return CoordinateType.MGRS;
        //        }
        //        catch { }
        //    }

        //    CoordinateUSNG usng;
        //    if (CoordinateUSNG.TryParse(input, out usng))
        //    {
        //        try
        //        {
        //            point = QueuedTask.Run(() =>
        //            {
        //                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //                var tmp = MapPointBuilder.FromGeoCoordinateString(usng.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.USNG, FromGeoCoordinateMode.Default);
        //                return tmp;
        //            }).Result;

        //            return CoordinateType.USNG;
        //        }
        //        catch { }
        //    }

        //    CoordinateUTM utm;
        //    if (CoordinateUTM.TryParse(input, out utm))
        //    {
        //        try
        //        {
        //            point = QueuedTask.Run(() =>
        //            {
        //                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
        //                var tmp = MapPointBuilder.FromGeoCoordinateString(utm.ToString("", new CoordinateUTMFormatter()), sptlRef, GeoCoordinateType.UTM, FromGeoCoordinateMode.Default);
        //                return tmp;
        //            }).Result;

        //            return CoordinateType.UTM;
        //        }
        //        catch { }
        //    }

        //    Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

        //    var matchMercator = regexMercator.Match(input);

        //    if (matchMercator.Success && matchMercator.Length == input.Length)
        //    {
        //        try
        //        {
        //            var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
        //            var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
        //            point = QueuedTask.Run(() =>
        //            {
        //                return MapPointBuilder.CreateMapPoint(Lon, Lat, SpatialReferences.WebMercator);
        //            }).Result;
        //            return CoordinateType.DD;
        //        }
        //        catch (Exception ex)
        //        {
        //            return CoordinateType.Unknown;
        //        }
        //    }

        //    return CoordinateType.Unknown;
        //}

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
                            return null;

                        var SelectedLayer = fl as BasicFeatureLayer;

                        var oidField = SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                        var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, SelectedOID) };
                        var cursor = SelectedLayer.Search(qf);
                        Row row = null;

                        if (cursor.MoveNext())
                            row = cursor.Current;

                        if (row == null)
                            return null;

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
                                        return temp;
                                        //// project to WGS1984
                                        //proCoordGetter.Point = temp;
                                        //proCoordGetter.Project(4326);
                                        //return string.Format("{0:0.0####} {1:0.0####}", proCoordGetter.Point.Y, proCoordGetter.Point.X);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    return null;
                });

                if(pointd != null)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NewMapPointSelection, pointd);

                //if (!string.IsNullOrWhiteSpace(pointd))
                //{
                //    InputCoordinate = pointd;
                //}
            }
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CoordinateConversionDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            CoordinateConversionDockpaneViewModel.Show();
        }
    }
}
