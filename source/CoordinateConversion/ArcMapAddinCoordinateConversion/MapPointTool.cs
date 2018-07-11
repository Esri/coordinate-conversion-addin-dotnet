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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using ArcMapAddinCoordinateConversion.Helpers;
using System.Windows.Forms;

namespace ArcMapAddinCoordinateConversion
{
    public class MapPointTool : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        ISnappingEnvironment m_SnappingEnv = null;
        IPointSnapper m_Snapper = null;
        ISnappingFeedback m_SnappingFeedback = null;

        public MapPointTool()
        {
            Mediator.Register(CoordinateConversionLibrary.Constants.CollectListHasItems, onCollectCoordinatesHasItems);
        }

        public bool ListHasItems { get; set; }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        protected override void OnActivate()
        {
            //Get the snap environment and initialize the feedback
            UID snapUID = new UID();
            this.Cursor = Cursors.Cross;
            snapUID.Value = "{E07B4C52-C894-4558-B8D4-D4050018D1DA}";

            if (ArcMap.Application != null)
                m_SnappingEnv = ArcMap.Application.FindExtensionByCLSID(snapUID) as ISnappingEnvironment;

            if (m_SnappingEnv != null)
                m_Snapper = m_SnappingEnv.PointSnapper;

            if (m_SnappingEnv != null)
            {
                m_SnappingFeedback = new SnappingFeedbackClass();
                m_SnappingFeedback.Initialize(ArcMap.Application, m_SnappingEnv, true);
            }
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            if (arg.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            try
            {
                var point = GetMapPoint(arg.X, arg.Y);
                if (point == null)
                    return;

                ISnappingResult snapResult = null;
                //Try to snap the current position
                if (m_Snapper != null)
                    snapResult = m_Snapper.Snap(point);

                if (m_SnappingFeedback != null)
                    m_SnappingFeedback.Update(null, 0);

                if (snapResult != null && snapResult.Location != null)
                    point = snapResult.Location;

                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NEW_MAP_POINT, point);
            }
            catch { }
        }

        protected override void OnMouseMove(MouseEventArgs arg)
        {
            try
            {
                var point = GetMapPoint(arg.X, arg.Y);
                if (point == null)
                    return;

                ISnappingResult snapResult = null;
                //Try to snap the current position
                snapResult = m_Snapper.Snap(point);
                m_SnappingFeedback.Update(snapResult, 0);
                if (snapResult != null && snapResult.Location != null)
                    point = snapResult.Location;

                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.MOUSE_MOVE_POINT, point);
                if (!ListHasItems)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);
            }
            catch { }
        }

        private void onCollectCoordinatesHasItems(object obj)
        {
            ListHasItems = (bool)obj;
        }

        private IPoint GetMapPoint(int X, int Y)
        {
            if ((ArcMap.Document == null) || (ArcMap.Document.FocusMap == null))
                return null;

            //Get the active view from the ArcMap static class.
            IActiveView activeView = (IActiveView)ArcMap.Document.FocusMap;

            var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y) as IPoint;
            if (point == null)
                return null;

            if (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType == CoordinateConversionLibrary.CoordinateTypes.None)
            {
                point.SpatialReference = ArcMap.Document.FocusMap.SpatialReference;
            }
            else
            {
                // always use WGS84
                var sr = ArcMapHelpers.GetGCS_WGS_1984_SR();

                if (sr != null)
                {
                    point.Project(sr);
                }
            }

            return point;
        }
    }
}
