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
using System.Windows.Forms;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ProAppCoordConversionModule.Models;
using CoordinateConversionLibrary.Views;
using System.IO;
using System.Text;
using System.Linq;

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProTabBaseViewModel : TabBaseViewModel
    {
        // This name should correlate to the name specified in Config.esriaddinx - Tool id="ProAppCoordConversionModule_CoordinateMapTool"
        internal const string MapPointToolName = "ProAppCoordConversionModule_CoordinateMapTool";

        public ProTabBaseViewModel()
        {
            ActivatePointToolCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnFlashPointCommandAsync);

            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            Mediator.Register("FLASH_COMPLETED", OnFlashCompleted);

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, proCoordGetter);

            ArcGIS.Desktop.Framework.Events.ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
        }

        public CoordinateConversionLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateConversionLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }

        public static ProCoordinateGet proCoordGetter = new ProCoordinateGet();
        public String PreviousTool { get; set; }

        private bool isToolActive = false;
        public bool IsToolActive
        {
            get
            {
                return isToolActive;
            }
            set
            {
                bool active = value;

                string toolToActivate = string.Empty;

                if (active)
                {
                    isToolActive = true;
                    string currentTool = FrameworkApplication.CurrentTool;
                    if (currentTool != MapPointToolName)
                    {
                        // Save previous tool to reactivate
                        PreviousTool = currentTool;
                        toolToActivate = MapPointToolName;
                    }
                }
                else
                {
                    isToolActive = false;

                    // Handle case if no Previous Tool
                    if (string.IsNullOrEmpty(PreviousTool))
                        PreviousTool = "esri_mapping_exploreTool";

                    toolToActivate = PreviousTool;
                }

                if (!string.IsNullOrEmpty(toolToActivate))
                {
                    FrameworkApplication.SetCurrentToolAsync(toolToActivate);
                }

                RaisePropertyChanged(() => IsToolActive);
            }
        }

        private static System.IDisposable _overlayObject = null;

        /// <summary>
        /// lists to store GUIDs of graphics, temp feedback and map graphics
        /// </summary>
        public static List<ProGraphic> ProGraphicsList = new List<ProGraphic>();

        private void ClearOverlay()
        {
            if (_overlayObject != null)
            {
                _overlayObject.Dispose();
                _overlayObject = null;
            }
        }

        #region overrides

        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            var mp = obj as MapPoint;

            if (mp == null)
                return false;

            proCoordGetter.Point = mp;
            InputCoordinate = proCoordGetter.GetInputDisplayString();

            return true;
        }

        public override bool OnMouseMove(object obj)
        {
            if (!base.OnMouseMove(obj))
                return false;

            var mp = obj as MapPoint;

            if (mp == null)
                return false;

            proCoordGetter.Point = mp;
            InputCoordinate = proCoordGetter.GetInputDisplayString();

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            if (proCoordGetter != null && proCoordGetter.Point != null)
            {
                InputCoordinate = proCoordGetter.GetInputDisplayString();
            }
        }

        public override void ProcessInput(string input)
        {
            if (input == "NA") return;

            string result = string.Empty;
            //MapPoint point;
            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return;

            //var coordType = GetCoordinateType(input, out point);
            // must force non async here to avoid returning to base class early
            var ccc = QueuedTask.Run(() =>
            {
                return GetCoordinateType(input);
            }).Result;


            if (ccc.Type == CoordinateType.Unknown)
            {
                HasInputError = true;
                proCoordGetter.Point = null;
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
                proCoordGetter.Point = ccc.Point;
                result = new CoordinateDD(ccc.Point.Y, ccc.Point.X).ToString("", new CoordinateDDFormatter());
            }

            return;
        }

        public Dictionary<string, string> GetOutputFormats(AddInPoint point)
        {
            var results = new Dictionary<string, string>();
            var ccc = QueuedTask.Run(() =>
            {
                return GetCoordinateType(point.Text);
            }).Result;
            if (ccc != null && ccc.Point != null)
            {
                ProCoordinateGet procoordinateGetter = new ProCoordinateGet();
                procoordinateGetter.Point = ccc.Point;
                CoordinateGetBase coordinateGetter = procoordinateGetter as CoordinateGetBase;

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

        public override void OnEditPropertiesDialogCommand(object obj)
        {
            var dlg = new ProEditPropertiesView();
            dlg.DataContext = new EditPropertiesViewModel();
            try
            {
                dlg.ShowDialog();
            }
            catch (Exception e)
            {
                if (e.Message.ToLower() == CoordinateConversionLibrary.Properties.Resources.CoordsOutOfBoundsMsg.ToLower())
                {
                    System.Windows.Forms.MessageBox.Show(e.Message + System.Environment.NewLine + CoordinateConversionLibrary.Properties.Resources.CoordsOutOfBoundsAddlMsg,
                        CoordinateConversionLibrary.Properties.Resources.CoordsoutOfBoundsCaption);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
        }

        public override void OnImportCSVFileCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.Filter = "csv files|*.csv";

            // attemp to import
            var fieldVM = new SelectCoordinateFieldsViewModel();
            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var dlg = new ProSelectCoordinateFieldsView();
                using (Stream s = new FileStream(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var headers = ImportCSV.GetHeaders(s);
                    foreach (var header in headers)
                    {
                        fieldVM.AvailableFields.Add(header);
                        System.Diagnostics.Debug.WriteLine("header : {0}", header);
                    }

                    dlg.DataContext = fieldVM;
                }
                if (dlg.ShowDialog() == true)
                {
                    using (Stream s = new FileStream(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var lists = ImportCSV.Import<ImportCoordinatesList>(s, fieldVM.SelectedFields.ToArray());
                        var coordinates = new List<string>();

                        foreach (var item in lists)
                        {
                            var sb = new StringBuilder();
                            sb.Append(item.lat.Trim());
                            if (fieldVM.UseTwoFields)
                                sb.Append(string.Format(" {0}", item.lon.Trim()));

                            coordinates.Add(sb.ToString());
                        }

                        Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, coordinates);
                    }
                }
            }

            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = true;
        }

        #endregion overrides

        #region Mediator handlers

        private void OnBCNeeded(object obj)
        {
            if (proCoordGetter == null || proCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(proCoordGetter.Point);
        }

        private void OnFlashCompleted(object obj)
        {
            IsToolActive = false;
            CoordinateMapTool.AllowUpdates = true;
        }

        #endregion Mediator handlers

        private void SetAsCurrentToolAsync()
        {
            IsToolActive = true;
        }

        private void OnMapToolCommand(object obj)
        {
            SetAsCurrentToolAsync();
        }

        private void OnActiveToolChanged(ArcGIS.Desktop.Framework.Events.ToolEventArgs args)
        {
            // Update active tool when tool changed so Map Point Tool button push state
            // stays in sync with Pro UI
            isToolActive = args.CurrentID == MapPointToolName;

            RaisePropertyChanged(() => IsToolActive);
        }

        internal async Task<string> AddGraphicToMap(Geometry geom, CIMColor color, bool IsTempGraphic = false, double size = 1.0, string text = "", SimpleMarkerStyle markerStyle = SimpleMarkerStyle.Circle, string tag = "")
        {
            if (geom == null || MapView.Active == null)
                return string.Empty;

            CIMSymbolReference symbol = null;

            if (!string.IsNullOrWhiteSpace(text) && geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                {
                    // TODO add text graphic
                    //var tg = new CIMTextGraphic() { Placement = Anchor.CenterPoint, Text = text};
                });
            }
            else if (geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                {
                    var s = SymbolFactory.Instance.ConstructPointSymbol(color, size, markerStyle);
                    var haloSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreenRGB);
                    haloSymbol.SetOutlineColor(ColorFactory.Instance.GreenRGB);
                    s.HaloSymbol = haloSymbol;
                    s.HaloSize = 0;
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                {
                    var s = SymbolFactory.Instance.ConstructLineSymbol(color, size);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.Instance.ConstructPolygonSymbol(color, SimpleFillStyle.Solid, outline);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }

            var result = await QueuedTask.Run(() =>
            {
                var disposable = MapView.Active.AddOverlay(geom, symbol);
                var guid = Guid.NewGuid().ToString();
                ProGraphicsList.Add(new ProGraphic(disposable, guid, geom, symbol, IsTempGraphic, tag));
                return guid;
            });

            return result;
        }

        internal async virtual void OnFlashPointCommandAsync(object obj)
        {
            var point = obj as MapPoint;

            if (point == null)
                return;

            IsToolActive = true;

            await QueuedTask.Run(() =>
            {
                // zoom to point
                var projectedPoint = GeometryEngine.Instance.Project(point, MapView.Active.Extent.SpatialReference);

                // WORKAROUND: delay zoom by 1 sec to give Map Point Tool enough time to activate
                // Note: The Map Point Tool is required to be active to enable flash overlay
                MapView.Active.PanTo(projectedPoint, new TimeSpan(0, 0, 1));

                Mediator.NotifyColleagues("UPDATE_FLASH", point);
            });
        }

        #region Private Methods

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

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);
        }

        private CoordinateType GetCoordinateType(string input, out MapPoint point)
        {
            point = null;

            // DD
            CoordinateDD dd;
            if (CoordinateDD.TryParse(input, out dd, true))
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
            if (CoordinateDDM.TryParse(input, out ddm, true))
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
            if (CoordinateDMS.TryParse(input, out dms, true))
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
            if (CoordinateMGRS.TryParse(input, out mgrs))
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

            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]*(?<longitude>\-?\d+[.,]?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
                    point = QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
                    }).Result;
                    return CoordinateType.DD;
                }
                catch (Exception ex)
                {
                    return CoordinateType.Unknown;
                }
            }

            return CoordinateType.Unknown;
        }
        private async Task<CCCoordinate> GetCoordinateType(string input)
        {
            MapPoint point = null;

            // DD
            CoordinateDD dd;
            if (CoordinateDD.TryParse(input, out dd, true))
            {
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm, true))
            {
                dd = new CoordinateDD(ddm);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DDM, Point = point };
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms, true))
            {
                dd = new CoordinateDD(dms);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DMS, Point = point };
            }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(gars.ToString("", new CoordinateGARSFormatter()), sptlRef, GeoCoordinateType.GARS, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.GARS, Point = point };
                }
                catch { }
            }

            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(mgrs.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.MGRS, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.MGRS, Point = point };
                }
                catch { }
            }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(usng.ToString("", new CoordinateMGRSFormatter()), sptlRef, GeoCoordinateType.USNG, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.USNG, Point = point }; ;
                }
                catch { }
            }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                        var tmp = MapPointBuilder.FromGeoCoordinateString(utm.ToString("", new CoordinateUTMFormatter()), sptlRef, GeoCoordinateType.UTM, FromGeoCoordinateMode.Default);
                        return tmp;
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.UTM, Point = point };
                }
                catch { }
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
                    var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
                    point = await QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
                    });//.Result;
                    return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
                }
                catch (Exception ex)
                {
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                }
            }

            return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
        }

        #endregion Private Methods

    }

    public class CCCoordinate
    {
        public CCCoordinate() { }
        public CoordinateType Type { get; set; }
        public MapPoint Point { get; set; }
    }
}
