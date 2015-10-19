using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Carto;
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

    public class PointTool : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public PointTool()
        {
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;

        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            if (arg.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            try
            {
                //Get the active view from the ArcMap static class.
                IActiveView activeView = ArcMap.Document.ActiveView;

                var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;

                // always use WGS84
                var sr = GetSR();

                if (sr != null)
                {
                    point.Project(sr);
                }

                var doc = AddIn.FromID<ArcMapAddinCoordinateTool.DockableWindowCoordinateTool.AddinImpl>(ThisAddIn.IDs.DockableWindowCoordinateTool);

                if (doc != null)
                {
                    doc.SetInput(point.X, point.Y);
                }
            }
            catch { }
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
