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

using System.Collections.ObjectModel;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System;
using ArcMapAddinCoordinateConversion.Helpers;
using ArcMapAddinCoordinateConversion.Models;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class ConvertTabViewModel : ArcMapTabBaseViewModel
    {
        public ConvertTabViewModel()
        {
            InputCCView = new InputCoordinateConversionView();
            InputCCView.DataContext = this;

            OutputCCView = new OutputCoordinateView();
            OutputCCView.DataContext = new OutputCoordinateViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new CollectTabViewModel();

            InputCoordinateHistoryList = new ObservableCollection<string>();
            Mediator.Register(CoordinateConversionLibrary.Constants.DEACTIVATE_TOOL, OnDeactivateTool);
        }      

        public InputCoordinateConversionView InputCCView { get; set; }
        public OutputCoordinateView OutputCCView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        private bool isToolActive = false;
        public bool IsToolActive
        {
            get
            {
                return isToolActive;
            }

            set
            {
                isSelectionToolActive = false;
                RaisePropertyChanged(() => IsSelectionToolActive);
                MapPointTool.SelectFeatureEnable = false;
                isToolActive = value;
                ActivateMapTool(value);
                RaisePropertyChanged(() => IsToolActive);
            }
        }
        private bool isSelectionToolActive = false;
        public bool IsSelectionToolActive
        {
            get
            {
                return isSelectionToolActive;
            }

            set
            {
                isToolActive = false;
                RaisePropertyChanged(() => IsToolActive);
                MapPointTool.SelectFeatureEnable = true;
                isSelectionToolActive = value;
                ActivateMapTool(value);
                RaisePropertyChanged(() => IsSelectionToolActive);
            }
        }
        private void ActivateMapTool(bool active)
        {
            string toolToActivate = string.Empty;
            if (active)
            {
                toolToActivate = MapPointToolName;
                //if (ArcMap.Application != null)
                //    CurrentTool = ArcMap.Application.CurrentTool;
            }
            else
            {
                //ArcMap.Application.CurrentTool = CurrentTool;
                toolToActivate = "esriCore.PanTool";
            }
            OnActivateTool(toolToActivate);
        }
        /// <summary>
        /// Activates the map tool to get map points from mouse clicks/movement
        /// </summary>
        /// <param name="obj"></param>
        internal void OnActivateTool(object obj)
        {
            var toolToActivate = obj as String;
            if (ArcMap.LayerCount > 0)
            {
                SetToolActiveInToolBar(ArcMap.Application, toolToActivate);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.AddLayerMsg,
                    CoordinateConversionLibrary.Properties.Resources.AddLayerCap);
            }
        }

        #region overrides

        /// <summary>
        /// Override to include the update of input coordinate history
        /// </summary>
        /// <param name="obj"></param>
        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            var formattedInputCoordinate = amCoordGetter.GetInputDisplayString();

            UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);

            // deactivate map point tool
            // KG - Commented out so user can continously capture coordinates
            //IsToolActive = false;

            // KG - Added so output component will updated when user clicks on the map 
            //      not when mouse move event is fired.
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

            return true;
        }

        internal override void OnFlashPointCommand(object obj)
        {
            if ((ArcMap.Document == null) || (ArcMap.Document.ActiveView == null) ||
                (ArcMap.Document.FocusMap == null))
                return;

            ProcessInput(InputCoordinate);
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

            IGeometry address = obj as IGeometry;
            if (address == null && amCoordGetter != null && amCoordGetter.Point != null)
            {
                address = amCoordGetter.Point;
                AddCollectionPoint(amCoordGetter.Point);
            }

            if (address != null)
            {
                IActiveView activeView = ArcMap.Document.ActiveView;
                IMap map = ArcMap.Document.FocusMap;
                IEnvelope envelope = activeView.Extent;

                IScreenDisplay screenDisplay = activeView.ScreenDisplay;
                short screenCache = Convert.ToInt16(esriScreenCache.esriNoScreenCache);

                ISpatialReference outgoingCoordSystem = map.SpatialReference;
                address.Project(outgoingCoordSystem);

                // is point within current extent
                // if so, pan to point
                var relationOp = envelope as IRelationalOperator;
                if (relationOp != null && activeView is IMap)
                {
                    if (!relationOp.Contains(address))
                    {
                        // pan to
                        envelope.CenterAt(address as IPoint);
                        activeView.Extent = envelope;
                        activeView.Refresh();
                    }
                }

                IRgbColor color = new RgbColorClass();
                color.Green = 80;
                color.Red = 22;
                color.Blue = 68;

                ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
                simpleMarkerSymbol.Color = color;
                simpleMarkerSymbol.Size = 15;
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

                IMarkerElement markerElement = new MarkerElementClass();

                markerElement.Symbol = simpleMarkerSymbol;

                IPolygon poly = null;
                if (InputCoordinateType == CoordinateType.MGRS || InputCoordinateType == CoordinateType.USNG)
                {
                    poly = GetMGRSPolygon(address as IPoint);
                }

                if (poly != null)
                {
                    address = poly;
                }
                ArcMapHelpers.FlashGeometry(address, color, activeView.ScreenDisplay, 500, activeView.Extent);
            }
        }

        #endregion overrides

        public void OnDeactivateTool(object obj)
        {
            IsToolActive = false;
        }

        private void AddCollectionPoint(IPoint point)
        {
            if (point != null && !point.IsEmpty)
            {
                var color = new RgbColorClass() { Red = 255 } as IColor;
                var guid = ArcMapHelpers.AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, ArcMapHelpers.DefaultMarkerSize);
                var addInPoint = new AddInPoint() { Point = point, GUID = guid };

                //Add point to the top of the list
                CollectTabViewModel.CoordinateAddInPoints.Insert(0, addInPoint);

                CollectTabViewModel.GraphicsList.Add(new AMGraphic(guid, point, true));
            }
        }
    }
}