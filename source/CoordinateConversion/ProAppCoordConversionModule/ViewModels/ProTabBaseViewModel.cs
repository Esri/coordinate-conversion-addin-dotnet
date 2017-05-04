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

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProTabBaseViewModel : TabBaseViewModel
    {
        // This name should correlate to the name specified in Config.esriaddinx - Tool id="Esri_ArcMapAddinCoordinateConversion_MapPointTool"
        internal const string MapPointToolName = "Esri_ArcMapAddinCoordinateConversion_MapPointTool";

        public ProTabBaseViewModel()
        {
            ActivatePointToolCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnFlashPointCommandAsync);

            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            Mediator.Register("FLASH_COMPLETED", OnFlashCompleted);

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, proCoordGetter);
        }

        public CoordinateConversionLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateConversionLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }

        public static ProCoordinateGet proCoordGetter = new ProCoordinateGet();
        public String PreviousTool { get; set; }

        public bool IsToolActive
        {
            get
            {
                if (FrameworkApplication.CurrentTool != null)
                    return FrameworkApplication.CurrentTool.ToLower() == MapPointToolName.ToLower();

                return false;
            }
            set
            {
                if (value)
                {
                    PreviousTool = FrameworkApplication.CurrentTool;
                    OnMapToolCommand(null);
                }     
                else
                    FrameworkApplication.SetCurrentToolAsync(PreviousTool);

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

        private async Task SetAsCurrentToolAsync()
        {	
            await FrameworkApplication.SetCurrentToolAsync("ProAppCoordConversionModule_CoordinateMapTool");

            RaisePropertyChanged(() => IsToolActive);
        }

        private async void OnMapToolCommand(object obj)
        {
            await SetAsCurrentToolAsync();
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
                    var s = SymbolFactory.ConstructPointSymbol(color, size, markerStyle);
                    var haloSymbol = SymbolFactory.ConstructPolygonSymbol(ColorFactory.GreenRGB);
                    haloSymbol.SetOutlineColor(ColorFactory.GreenRGB);
                    s.HaloSymbol = haloSymbol;
                    s.HaloSize = 0;
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                {
                    var s = SymbolFactory.ConstructLineSymbol(color, size);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var outline = SymbolFactory.ConstructStroke(ColorFactory.BlackRGB, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.ConstructPolygonSymbol(color, SimpleFillStyle.Solid, outline);
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

            if (!IsToolActive)
            {
                await SetAsCurrentToolAsync();
            }

            await QueuedTask.Run(() =>
            {
                // is point within current map extent
                var projectedPoint = GeometryEngine.Project(point, MapView.Active.Extent.SpatialReference);
                if (!GeometryEngine.Contains(MapView.Active.Extent, projectedPoint))
                {
                    MapView.Active.PanTo(point);
                }
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
            if (CoordinateDD.TryParse(input, out dd))
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
            if (CoordinateDDM.TryParse(input, out ddm))
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
            if (CoordinateDD.TryParse(input, out dd))
            {
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm))
            {
                dd = new CoordinateDD(ddm);
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DDM, Point = point };
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms))
            {
                dd = new CoordinateDD(dms);
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
             * Commented out this section of code since it does not capture invalid coordinates
             * like 00, 45, or 456987. 
             * 
             * TODO: update RegEx to accommodate for lack of delimeter
             */
            //Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]*(?<longitude>\-?\d+[.,]?\d*)");

            //var matchMercator = regexMercator.Match(input);

            //if (matchMercator.Success && matchMercator.Length == input.Length)
            //{
            //    try
            //    {
            //        var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
            //        var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
            //        var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
            //        point = await QueuedTask.Run(() =>
            //        {
            //            return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
            //        });//.Result;
            //        return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
            //    }
            //    catch (Exception ex)
            //    {
            //        return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
            //    }
            //}

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
