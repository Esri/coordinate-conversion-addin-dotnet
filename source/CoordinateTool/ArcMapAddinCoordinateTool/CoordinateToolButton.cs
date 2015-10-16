using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Desktop.AddIns;

namespace ArcMapAddinCoordinateTool
{
    public class CoordinateToolButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public CoordinateToolButton()
        {
        }

        protected override void OnClick()
        {
            ArcMap.Application.CurrentTool = null;

            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.DockableWindowCoordinateTool;

            IDockableWindow dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
            dockWindow.Show(true);
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

    public class Tool1 : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public Tool1()
        {
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;

        }


        #region"MapControl Left and Right Mouse Clicks"
        // ArcGIS Snippet Title:
        // MapControl Left and Right Mouse Clicks
        // 
        // Long Description:
        // Stub code for using left and right mouse clicks for ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent.
        // 
        // Add the following references to the project:
        // ESRI.ArcGIS.AxControls
        // ESRI.ArcGIS.Controls
        // 
        // Intended ArcGIS Products for this snippet:
        // ArcGIS Engine
        // 
        // Applicable ArcGIS Product Versions:
        // 9.2
        // 9.3
        // 9.3.1
        // 10.0
        // 
        // Required ArcGIS Extensions:
        // (NONE)
        // 
        // Notes:
        // This snippet is intended to be inserted at the base level of a Class.
        // It is not intended to be nested within an existing Method.
        // 

        ///<summary>Stub code for using left and right mouse clicks for ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent.</summary>
        /// 
        ///<param name="e">An ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent.</param>
        /// 
        ///<remarks>e is obtained as the result of the AxMapControl.OnMouseDown event.</remarks>
        //public void MapControlLeftAndRightMouseClicks(ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        //{

        //    if (e.button == 1) // Left mouse click
        //    {

        //        // TODO: Your implementation code.....

        //    }

        //    if (e.button == 2) // Right mouse click
        //    {

        //        // TODO: Your implementation code.....

        //    }

        //}
        #endregion

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            var doc = AddIn.FromID<ArcMapAddinCoordinateTool.DockableWindowCoordinateTool.AddinImpl>(ThisAddIn.IDs.DockableWindowCoordinateTool);

            if(doc != null)
            {
                doc.TestMe();
            }
            //Get the active view from the ArcMap static class.
            IActiveView activeView = ArcMap.Document.ActiveView;
            var mc2 = activeView.FocusMap as IMapControl2;
            if (mc2 != null)
            {
                var point2 = mc2.ToMapPoint(arg.X, arg.Y);
            }

            var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;

            var sr = GetSR();

            point.Project(sr);

            Console.WriteLine(string.Format("{0} {1}", point.X, point.Y));

            double x = point.X;
            double y = point.Y;

            var temp = activeView.FocusMap.MapUnits;

            //If it's a polyline object, get from the user's mouse clicks.
            //IPolyline polyline = GetPolylineFromMouseClicks(activeView);
            ////Make a color to draw the polyline. 

            //IRgbColor rgbColor = new RgbColorClass();
            //rgbColor.Red = 255;
            ////Add the user's drawn graphics as persistent on the map.
            //AddGraphicToMap(activeView.FocusMap, polyline, rgbColor, rgbColor);
            ////Best practice: Redraw only the portion of the active view that contains graphics. 
            //activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
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

    }


}
