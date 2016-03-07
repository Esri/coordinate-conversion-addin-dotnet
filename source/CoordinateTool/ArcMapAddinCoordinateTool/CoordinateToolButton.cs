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
                var point = GetMapPoint(arg.X, arg.Y);

                var doc = AddIn.FromID<ArcMapAddinCoordinateTool.DockableWindowCoordinateTool.AddinImpl>(ThisAddIn.IDs.DockableWindowCoordinateTool);

                if (doc != null && point != null)
                {
                    doc.SetInput(point.X, point.Y);
                }

                ArcMap.Application.CurrentTool = null;

                doc.GetMainVM().UpdateButtonState();
            }
            catch { }
        }

        protected override void OnMouseMove(MouseEventArgs arg)
        {
            try
            {
                IPoint point = GetMapPoint(arg.X, arg.Y);

                var doc = AddIn.FromID<ArcMapAddinCoordinateTool.DockableWindowCoordinateTool.AddinImpl>(ThisAddIn.IDs.DockableWindowCoordinateTool);

                if (doc != null && point != null)
                {
                    doc.SetInput(point.X, point.Y);
                }
            }
            catch { }
        }

        private IPoint GetMapPoint(int X, int Y)
        {
            //Get the active view from the ArcMap static class.
            IActiveView activeView = ArcMap.Document.FocusMap as IActiveView;

            var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y) as IPoint;

            // always use WGS84
            var sr = GetSR();

            if (sr != null)
            {
                point.Project(sr);
            }

            return point;
        }

        private ISpatialReference GetSR()
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

    }

}
