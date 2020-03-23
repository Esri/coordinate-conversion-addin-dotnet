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

using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ProAppCoordConversionModule.UI;
using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.Models;
using Constants = ProAppCoordConversionModule.Helpers.Constants;
using ProAppCoordConversionModule.Common.Enums;


namespace ProAppCoordConversionModule
{
    internal class CoordinateMapTool : MapTool
    {
        public static bool AllowUpdates = true;
        public static bool SelectFeatureEnable = false;

        public static string ToolId
        {
            // Important: this must match the Tool ID used in the DAML
            get { return "ProAppCoordConversionModule_CoordinateMapTool"; }
        }

        public CoordinateMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            UseSnapping = true;

            Module1.coordMapTool = this;

            //Set the tools OverlayControlID to the DAML id of the embeddable control
            OverlayControlID = "ProAppCoordConversionModule_EmbeddableControl";

            PointFlash = new RelayCommand(OnUpdateFlash);
            CollectCoordinatesHasItems = new RelayCommand(onCollectCoordinatesHasItems);
        }

        public bool ListHasItems { get; set; }

        public RelayCommand CollectCoordinatesHasItems { get; set; }

        public RelayCommand PointFlash { get; set; }

        protected override Task OnToolActivateAsync(bool active)
        {
            ContextMenuID = "esri_mapping_popupToolContextMenu";
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mp = geometry as MapPoint;

            //Get the instance of the Main ViewModel from the dock pane

            CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
            ViewModels.ProConvertTabViewModel pCvtTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;

            if (SelectFeatureEnable)
            {
                ViewModels.ProCollectTabViewModel pCollectTabVM = pCvtTabVM.CollectTabView.DataContext as ViewModels.ProCollectTabViewModel;
                pCollectTabVM.SelectMapPointInternal.Execute(mp);
            }
            else
            {
                pCvtTabVM.ValidateMapPointInternal.Execute(mp);

                ViewModels.ProCollectTabViewModel pCollectTabVM = pCvtTabVM.CollectTabView.DataContext as ViewModels.ProCollectTabViewModel;
                pCollectTabVM.ValidateMapPointInternal.Execute(mp);
            }

            return base.OnSketchCompleteAsync(geometry);
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            UpdateInputWithMapPoint(e.ClientPoint);
        }

        private void OnUpdateFlash(object obj)
        {
            var mp = obj as MapPoint;

            if (mp != null)
                UpdateFlash(mp);
        }

        private void onCollectCoordinatesHasItems(object obj)
        {
            ListHasItems = (bool)obj;
        }

        private void UpdateFlash(MapPoint mp)
        {
            System.Windows.Point? temp = new System.Windows.Point();

            var cp = QueuedTask.Run(() =>
            {
                if (MapView.Active != null)
                {
                    temp = MapView.Active.MapToClient(mp);
                }
                return temp;
            }).Result as System.Windows.Point?;

            UpdateFlash(cp);
        }

        private void UpdateFlash(System.Windows.Point? point)
        {
            var flashVM = OverlayEmbeddableControl as FlashEmbeddedControlViewModel;
            if (flashVM != null)
                flashVM.ClientPoint = point.Value;

            var temp = QueuedTask.Run(() =>
                {
                    if (flashVM != null && MapView.Active != null)
                    {
                        flashVM.ScreenPoint = MapView.Active.ClientToScreen(point.Value);
                        var p1 = MapView.Active.MapToScreen(MapPointBuilder.CreateMapPoint(MapView.Active.Extent.XMin, MapView.Active.Extent.YMin));
                        var p3 = MapView.Active.MapToScreen(MapPointBuilder.CreateMapPoint(MapView.Active.Extent.XMax, MapView.Active.Extent.YMax));
                        var width = (p3.X - p1.X) + 1;
                        var height = (p1.Y - p3.Y) + 1;
                        flashVM.MapWidth = width;
                        flashVM.MapHeight = height;
                        flashVM.RunFlashAnimation();
                    }
                    return true;
                }).Result;
        }

        /// <summary>
        /// Method to update the input coordinate text box
        /// </summary>
        /// <param name="mp">MapPoint</param>
        private void UpdateInputWithMapPoint(MapPoint mp)
        {
            if (AllowUpdates)
            {
                if (mp != null)
                {
                    if (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType != CoordinateTypes.None)
                        mp = GeometryEngine.Instance.Project(mp, SpatialReferences.WGS84) as MapPoint;

                    CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
                    ViewModels.ProConvertTabViewModel pCvtTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;
                    pCvtTabVM.MouseMoveInternal.Execute(mp);
                }
            }
        }

        /// <summary>
        /// Method to update the input coordinate text box
        /// </summary>
        /// <param name="e"></param>
        private async void UpdateInputWithMapPoint(System.Windows.Point e)
        {
            if (AllowUpdates)
            {
                var mp = await QueuedTask.Run(() =>
                {
                    MapPoint temp = null;

                    if (MapView.Active != null)
                    {
                        temp = MapView.Active.ClientToMap(e);
                        try
                        {
                            // for now we will always project to WGS84
                            if (CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType != CoordinateTypes.None
                                && CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType != CoordinateTypes.Default)
                                temp = GeometryEngine.Instance.Project(temp, SpatialReferences.WGS84) as MapPoint;

                            return temp;
                        }
                        catch { /* Projection Failed */ }
                    }

                    return temp;
                });//.Result as MapPoint;

                if (mp != null)
                {
                    CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
                    ViewModels.ProConvertTabViewModel pCvtTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;
                    pCvtTabVM.MouseMoveInternal.Execute(mp);
                    
                    if (!ListHasItems)
                    {
                        ViewModels.ProOutputCoordinateViewModel pOutCoordVM = pCvtTabVM.OutputCCView.DataContext as ViewModels.ProOutputCoordinateViewModel;
                        pOutCoordVM.RequestOutputCommand.Execute(null);
                    }
                }
            }
        }
    }
}
