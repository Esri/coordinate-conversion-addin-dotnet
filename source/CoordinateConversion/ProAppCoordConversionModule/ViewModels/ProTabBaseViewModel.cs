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
        public ProTabBaseViewModel()
        {
            ActivatePointToolCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnFlashPointCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, proCoordGetter);
        }

        public CoordinateConversionLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateConversionLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }

        public static ProCoordinateGet proCoordGetter = new ProCoordinateGet();

        public bool IsToolActive
        {
            get
            {
                if (FrameworkApplication.CurrentTool != null)
                    return FrameworkApplication.CurrentTool == "ProAppCoordConversionModule_CoordinateMapTool";

                return false;
            }
            set
            {
                if (value)
                    OnMapToolCommand(null);
                else
                    FrameworkApplication.SetCurrentToolAsync(string.Empty);

                RaisePropertyChanged(() => IsToolActive);
                //TODO remove if not needed
                //NotifyPropertyChanged(new PropertyChangedEventArgs("IsToolActive"));
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

        public override string ProcessInput(string input)
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
            }

            return result;
        }

        #endregion overrides

        #region Mediator handlers

        private void OnBCNeeded(object obj)
        {
            if (proCoordGetter == null || proCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(proCoordGetter.Point);
        }

        #endregion Mediator handlers

        private void OnMapToolCommand(object obj)
        {
            FrameworkApplication.SetCurrentToolAsync("ProAppCoordConversionModule_CoordinateMapTool");
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


        internal async virtual void OnFlashPointCommand(object obj)
        {
            var point = obj as MapPoint;

            if(point == null)
                return;

            var previous = IsToolActive;
            if (!IsToolActive)
            {
                IsToolActive = true;
            }

            //TODO fix this
            //CoordinateDD dd;
            
            //if (!CoordinateDD.TryParse(ListBox ctvm.InputCoordinate, out dd))
            //{
            //    Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

            //    var matchMercator = regexMercator.Match(ctvm.InputCoordinate);

            //    if (matchMercator.Success && matchMercator.Length == ctvm.InputCoordinate.Length)
            //    {
            //        try
            //        {
            //            var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
            //            var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
            //            point = QueuedTask.Run(() =>
            //            {
            //                return MapPointBuilder.CreateMapPoint(Lon, Lat, SpatialReferences.WebMercator);
            //            }).Result;
            //        }
            //        catch (Exception ex)
            //        {
            //            // do nothing
            //        }
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

            //ArcGIS.Core.CIM.CIMPointSymbol symbol = null;

            //TODO fix this also
            //if (point == null)
            //{
            //    point = await QueuedTask.Run(() =>
            //    {
            //        ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
            //        return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
            //    });
            //}

            ////

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

            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    point = QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, SpatialReferences.WebMercator);
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

        #endregion Private Methods

    }
}
