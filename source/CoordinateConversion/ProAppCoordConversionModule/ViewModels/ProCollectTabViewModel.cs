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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateConversionLibrary.Helpers;
using ProAppCoordConversionModule.Models;
using ProAppCoordConversionModule.Views;
using ProAppCoordConversionModule.ViewModels;
using CoordinateConversionLibrary;
using System;
using System.Text;
using Jitbit.Utils;

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
            SaveAsCommand = new RelayCommand(OnSaveAsCommand);
            CopyCoordinateCommand = new RelayCommand(OnCopyCommand);
            CopyAllCoordinatesCommand = new RelayCommand(OnCopyAllCommand);

            // Listen to collection changed event and notify colleagues
            CoordinateAddInPoints.CollectionChanged += CoordinateAddInPoints_CollectionChanged;

            Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
            Mediator.Register(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, OnImportCoordinates);
        }

        /// <summary>
        /// Notify if collection list has any items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoordinateAddInPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.CollectListHasItems, CoordinateAddInPoints.Any());
        }

        private void OnImportCoordinates(object obj)
        {
            var coordinates = obj as List<string>;

            if (coordinates == null)
                return;

            foreach (var coordinate in coordinates)
            {
                this.ProcessInput(coordinate);
                InputCoordinate = coordinate;
                if (!HasInputError)
                    OnNewMapPoint(proCoordGetter.Point);
            }
            InputCoordinate = "";
        }

        public bool HasListBoxRightClickSelectedItem 
        {
            get 
            {
                return ListBoxItemAddInPoint != null;
            }
        }
        public bool HasAnySelectedItems
        {
            get
            {
                return CoordinateAddInPoints.Any(p => p.IsSelected == true);
            }
        }

        public AddInPoint ListBoxItemAddInPoint { get; set; }

        public static ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }

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
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand CopyCoordinateCommand { get; set; }
        public RelayCommand CopyAllCoordinatesCommand { get; set; }

        private void OnDeletePointCommand(object obj)
        {
            var items = obj as IList;
            var objects = items.Cast<AddInPoint>().ToList();

            DeletePoints(objects);
        }

        private void OnDeleteAllPointsCommand(object obj)
        {
            DeletePoints(CoordinateAddInPoints.ToList());
        }

        private void OnClearGraphicsCommand(object obj)
        {
            DeletePoints(CoordinateAddInPoints.ToList());
        }

        private void OnEnterKeyCommand(object obj)
        {
            if (!HasInputError && InputCoordinate.Length > 0)
            {
                AddCollectionPoint(proCoordGetter.Point);
            }
        }

        // Call CopyAllCoordinateOutputs event
        private void OnCopyAllCommand(object obj)
        {
            OnCopyAllCoordinateOutputs(CoordinateAddInPoints.ToList());
        }

        // copy parameter to clipboard
        private void OnCopyCommand(object obj)
        {
            var items = obj as IList;
            var objects = items.Cast<AddInPoint>().ToList();

            if (!objects.Any())
                return;

            var sb = new StringBuilder();
            foreach (var point in objects)
            {
                sb.AppendLine(point.Text);
            }

            if (sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }

        private async void OnSaveAsCommand(object obj)
        {
            var saveAsDialog = new ProSaveAsFormatView();
            var vm = new ProSaveAsFormatViewModel();
            saveAsDialog.DataContext = vm;

            if(saveAsDialog.ShowDialog() == true)
            {
                var fcUtils = new FeatureClassUtils();

                string path = fcUtils.PromptUserWithSaveDialog(vm.FeatureIsChecked, vm.ShapeIsChecked, vm.KmlIsChecked, vm.CSVIsChecked);
                if (path != null)
                {
                    try
                    {
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        var mapPointList = CoordinateAddInPoints.Select(i => i.Point).ToList();
                        if (vm.FeatureIsChecked)
                        {
                            await fcUtils.CreateFCOutput(path, 
                                                         SaveAsType.FileGDB,
                                                         mapPointList, 
                                                         MapView.Active.Map.SpatialReference, 
                                                         MapView.Active, 
                                                         CoordinateConversionLibrary.GeomType.Point);
                        }
                        else if (vm.ShapeIsChecked || vm.KmlIsChecked)
                        {
                            await fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, mapPointList, MapView.Active.Map.SpatialReference, MapView.Active, CoordinateConversionLibrary.GeomType.Point, vm.KmlIsChecked);
                        }
                        else if (vm.CSVIsChecked)
                        {
                            var aiPoints = CoordinateAddInPoints.ToList();

                            if (!aiPoints.Any())
                                return;

                            var csvExport = new CsvExport();
                            foreach (var point in aiPoints)
                            {
                                csvExport.AddRow();
                                csvExport["Coordinate"] = point.Text;
                            }
                            csvExport.ExportToFile(path);

                            System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.CSVExportSuccessfulMessage + path,
                                CoordinateConversionLibrary.Properties.Resources.CSVExportSuccessfulCaption); 
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Copies all coordinates to the clipboard
        /// </summary>
        /// <param name="obj"></param>
        private void OnCopyAllCoordinateOutputs(List<AddInPoint> aiPoints)
        {
            var sb = new StringBuilder();

            if (aiPoints == null || !aiPoints.Any())
                return;

            foreach (var point in aiPoints)
            {
                sb.AppendLine(point.Text);
            }

            if (sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }

        private async void UpdateHighlightedGraphics()
        {
            var list = ProGraphicsList.ToList();
            foreach (var proGraphic in list)
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
            var guid = await AddGraphicToMap(point, ColorFactory.Instance.RedRGB, true, 7);
            var addInPoint = new AddInPoint() { Point = point, GUID = guid };

            //Add point to the top of the list
            CoordinateAddInPoints.Insert(0, addInPoint);
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
            RaisePropertyChanged(() => HasListBoxRightClickSelectedItem);
        }

        #region overrides

        internal override void OnFlashPointCommandAsync(object obj)
        {
            if (ListBoxItemAddInPoint != null)
            {
                var geometry = ListBoxItemAddInPoint.Point;
                ListBoxItemAddInPoint = null;

                base.OnFlashPointCommandAsync(geometry);
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
