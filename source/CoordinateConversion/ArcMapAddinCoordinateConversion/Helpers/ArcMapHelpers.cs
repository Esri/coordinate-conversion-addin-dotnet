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
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace ArcMapAddinCoordinateConversion.Helpers
{
    public class ArcMapHelpers
    {
        public static ISpatialReference GetGCS_WGS_1984_SR()
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
        public static ISpatialReference GetSR(int type)
        {
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.
            try
            {
                IGeographicCoordinateSystem geographicCS = srFact.CreateGeographicCoordinateSystem(type);

                return geographicCS as ISpatialReference;
            }
            catch (Exception ex)
            {
                // do nothing
            }


            try
            {
                IProjectedCoordinateSystem projectCS = srFact.CreateProjectedCoordinateSystem(type);

                return projectCS as ISpatialReference;
            }
            catch (Exception ex)
            {
                // do nothing
            }

            return null;
        }
        /// <summary>
        /// Adds a graphic element to the map graphics container
        /// Returns GUID
        /// </summary>
        /// <param name="geom">IGeometry</param>
        public static string AddGraphicToMap(IGeometry geom, IColor color, bool IsTempGraphic = false, esriSimpleMarkerStyle markerStyle = esriSimpleMarkerStyle.esriSMSCircle, int size = 5)
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

            gc.AddElement(element, 0);

            av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            return eprop.Name;
        }

        ///<summary>Flash geometry on the display. The geometry type could be polygon, polyline, point, or multipoint.</summary>
        ///
        ///<param name="geometry"> An IGeometry interface</param>
        ///<param name="color">An IRgbColor interface</param>
        ///<param name="display">An IDisplay interface</param>
        ///<param name="delay">A System.Int32 that is the time im milliseconds to wait.</param>
        /// 
        ///<remarks></remarks>
        public static void FlashGeometry(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IRgbColor color, ESRI.ArcGIS.Display.IDisplay display, System.Int32 delay, IEnvelope envelope)
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

                        ArcMapHelpers.DrawCrossHair(geometry, display, envelope, markerSymbol, lineSymbol);

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

    }
}
