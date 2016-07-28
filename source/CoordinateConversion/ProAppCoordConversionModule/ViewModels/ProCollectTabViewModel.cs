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

using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateConversionLibrary.Helpers;
using ProAppCoordConversionModule.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO implement this view model
namespace ProAppCoordConversionModule.ViewModels
{
    public class ProCollectTabViewModel : ProTabBaseViewModel
    {
        public ProCollectTabViewModel()
        {
            ListBoxItemAddInPoint = null;

            CoordinateAddInPoints = new ObservableCollection<AddInPoint>();

            DeletePointCommand = new RelayCommand(OnDeletePointCommand);
            DeleteAllPointsCommand = new RelayCommand(OnDeleteAllPointsCommand);
            ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);
            EnterKeyCommand = new RelayCommand(OnEnterKeyCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
        }

        public AddInPoint ListBoxItemAddInPoint { get; set; }

        public ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }


        private object _ListBoxSelectedItem = null;
        public object ListBoxSelectedItem
        {
            get { return _ListBoxSelectedItem; }
            set
            {
                // we are using this to know when the selection changes
                // setting null will allow this setter to be called in multiple selection mode
                _ListBoxSelectedItem = null;
                RaisePropertyChanged(() => ListBoxSelectedItem);

                // update selections
                UpdateHighlightedGraphics();
            }
        }

        private int _ListBoxSelectedIndex = -1;
        public int ListBoxSelectedIndex
        {
            get { return _ListBoxSelectedIndex; }
            set
            {
                // this works with the ListBoxSelectedItem 
                // without this the un-selecting of 1 will not trigger an update
                _ListBoxSelectedIndex = value;
                RaisePropertyChanged(() => ListBoxSelectedIndex);
                UpdateHighlightedGraphics();
            }
        }

        public RelayCommand DeletePointCommand { get; set; }
        public RelayCommand DeleteAllPointsCommand { get; set; }
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand EnterKeyCommand { get; set; }

        // lists to store GUIDs of graphics, temp feedback and map graphics
        //private static List<ProGraphic> ProGraphicsList = new List<ProGraphic>();

        private void OnDeletePointCommand(object obj)
        {
            var items = obj as IList;
            var objects = items.Cast<AddInPoint>().ToList();

            if (objects == null)
                return;

            DeletePoints(objects);
        }

        private void OnDeleteAllPointsCommand(object obj)
        {
            DeletePoints(CoordinateAddInPoints.ToList());
        }

        private void OnClearGraphicsCommand(object obj)
        {
            //TODO update this to Pro
            //var mxdoc = ArcMap.Application.Document as IMxDocument;
            //if (mxdoc == null)
            //    return;
            //var av = mxdoc.FocusMap as IActiveView;
            //if (av == null)
            //    return;
            //var gc = av as IGraphicsContainer;
            //if (gc == null)
            //    return;
            ////TODO need to clarify what clear graphics button does
            //// seems to be different than the other Military Tools when doing batch collection
            //RemoveGraphics(gc, ProGraphicsList.Where(g => g.IsTemp == false).ToList());

            ////av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            //av.Refresh(); // sometimes a partial refresh is not working
        }

        private void OnEnterKeyCommand(object obj)
        {
            if (!HasInputError && InputCoordinate.Length > 0)
            {
                AddCollectionPoint(proCoordGetter.Point);
            }
        }

        //TODO fix this for Pro
        /// <summary>
        /// Method used to remove graphics from the graphics container
        /// Elements are tagged with a GUID on the IElementProperties.Name property
        /// </summary>
        /// <param name="gc">map graphics container</param>
        /// <param name="list">list of GUIDs to remove</param>
        //private void RemoveGraphics(IGraphicsContainer gc, List<AMGraphic> list)
        //{
        //    if (gc == null || !list.Any())
        //        return;

        //    var elementList = new List<IElement>();
        //    gc.Reset();
        //    var element = gc.Next();
        //    while (element != null)
        //    {
        //        var eleProps = element as IElementProperties;
        //        if (list.Any(g => g.UniqueId == eleProps.Name))
        //        {
        //            elementList.Add(element);
        //        }
        //        element = gc.Next();
        //    }

        //    foreach (var ele in elementList)
        //    {
        //        gc.DeleteElement(ele);
        //    }

        //    // remove from master graphics list
        //    foreach (var graphic in list)
        //    {
        //        if (ProGraphicsList.Contains(graphic))
        //            ProGraphicsList.Remove(graphic);
        //    }
        //    elementList.Clear();
        //}

        private async void UpdateHighlightedGraphics()
        {
            foreach (var proGraphic in ProGraphicsList)
            {
                var aiPoint = CoordinateAddInPoints.FirstOrDefault(p => p.GUID == proGraphic.GUID);

                if (aiPoint != null)
                {
                    var s = proGraphic.SymbolRef.Symbol as CIMPointSymbol;

                    if (s == null)
                        continue;

                    if (aiPoint.IsSelected)
                        s.HaloSize = 2;
                    else
                        s.HaloSize = 0;
                    
                    var result = await QueuedTask.Run(() =>
                    {
                        var temp = MapView.Active.UpdateOverlay(proGraphic.Disposable, proGraphic.Geometry, proGraphic.SymbolRef);
                        return temp;
                    });

                }
            }
        }

        private async void AddCollectionPoint(MapPoint point)
        {
            var guid = await AddGraphicToMap(point, ColorFactory.RedRGB, true, 7);
            var addInPoint = new AddInPoint() { Point = point, GUID = guid };
            CoordinateAddInPoints.Add(addInPoint);
        }

        private void RemoveGraphics(List<string> guidList)
        {
            var list = ProGraphicsList.Where(g => guidList.Contains(g.GUID)).ToList();
            foreach (var graphic in list)
            {
                if (graphic.Disposable != null)
                    graphic.Disposable.Dispose();
                ProGraphicsList.Remove(graphic);
            }

            //TODO do we still use HasMapGraphics?
            //RaisePropertyChanged(() => HasMapGraphics);
        }

        private void DeletePoints(List<AddInPoint> aiPoints)
        {
            if (aiPoints == null || !aiPoints.Any())
                return;

            // remove graphics from map
            var guidList = aiPoints.Select(x => x.GUID).ToList();
            RemoveGraphics(guidList);

            foreach (var point in aiPoints)
            {
                CoordinateAddInPoints.Remove(point);
            }
        }

        private void OnSetListBoxItemAddInPoint(object obj)
        {
            ListBoxItemAddInPoint = obj as AddInPoint;
        }

        //TODO fix this for Pro
        //private void ClearGraphicsContainer(IMap map)
        //{
        //    var graphicsContainer = map as IGraphicsContainer;
        //    if (graphicsContainer != null)
        //    {
        //        //graphicsContainer.DeleteAllElements();
        //        // now we have a collection feature and need to not clear those related graphics
        //        graphicsContainer.Reset();
        //        var g = graphicsContainer.Next();
        //        while (g != null)
        //        {
        //            if (!CoordinateAddInPoints.Any(aiPoint => aiPoint.GUID == ((IElementProperties)g).Name))
        //                graphicsContainer.DeleteElement(g);
        //            g = graphicsContainer.Next();
        //        }
        //    }
        //}


        #region overrides

        internal override void OnFlashPointCommand(object obj)
        {
            if (ListBoxItemAddInPoint != null)
            {
                var geometry = ListBoxItemAddInPoint.Point;
                ListBoxItemAddInPoint = null;

                base.OnFlashPointCommand(geometry);
            }
        }

        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;

            AddCollectionPoint(obj as MapPoint);

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibrary.Models.CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            // update list box coordinates
            var list = CoordinateAddInPoints.ToList();
            CoordinateAddInPoints.Clear();
            foreach (var item in list)
                CoordinateAddInPoints.Add(item);
        }

        #endregion overrides
    }
}
