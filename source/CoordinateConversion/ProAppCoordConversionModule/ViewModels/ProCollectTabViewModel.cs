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

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ProAppCoordConversionModule.Common;
using ProAppCoordConversionModule.Models;
using Jitbit.Utils;
using ProAppCoordConversionModule.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            PasteCoordinatesCommand = new RelayCommand(OnPasteCommand);

            // Listen to collection changed event and notify colleagues
            CoordinateAddInPoints.CollectionChanged += CoordinateAddInPoints_CollectionChanged;
            Mediator.Register(Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
            Mediator.Register(Constants.IMPORT_COORDINATES, OnImportCoordinates);
        }

        /// <summary>
        /// Notify if collection list has any items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoordinateAddInPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Mediator.NotifyColleagues(Constants.CollectListHasItems, CoordinateAddInPoints.Any());
        }

        private void OnImportCoordinates(object obj)
        {
            var progressDialog = new ProgressDialog("Processing.. Please wait");
            progressDialog.Show();

            try
            {
                IsToolActive = false;
                if (obj == null)
                    return;
                var input = obj as List<Dictionary<string, Tuple<object, bool>>>;
                if (input != null)
                    foreach (var item in input)
                    {
                        var coordinate = item.Where(x => x.Key == OutputFieldName).Select(x => Convert.ToString(x.Value.Item1)).FirstOrDefault();
                        if (coordinate == "" || item.Where(x => x.Key == PointFieldName).Any())
                            continue;
                        this.ProcessInputValue(coordinate);
                        InputCoordinate = coordinate;

                        if (!HasInputError)
                        {
                            if (item.ContainsKey(PointFieldName))
                                continue;
                            item.Add(PointFieldName, Tuple.Create((object)proCoordGetter.Point, false));
                            OnNewMapPoint(item);
                        }
                    }
                else
                {
                    List<string> coordinates = obj as List<string>;
                    if (coordinates == null)
                        return;
                    this.ClearListBoxSelection();
                    foreach (var coordinate in coordinates)
                    {
                        ProcessInputValue(coordinate);
                        InputCoordinate = coordinate;
                        if (!HasInputError)
                            OnNewMapPoint(proCoordGetter.Point);
                    }

                    InputCoordinate = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed in OnImportCoordinates: " + ex.Message);
            }

            progressDialog.Hide();
        }

        private void ClearListBoxSelection()
        {
            UpdateHighlightedGraphics(true);
            Mediator.NotifyColleagues(Constants.CollectListHasItems, CoordinateAddInPoints.Any());
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
                UpdateHighlightedGraphics(false);

                var addinPoint = CoordinateAddInPoints.Where(x => x.IsSelected).FirstOrDefault();
                proCoordGetter.Point = addinPoint.Point;
                Mediator.NotifyColleagues(Constants.RequestOutputUpdate, null);
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

            }
        }

        public RelayCommand DeletePointCommand { get; set; }
        public RelayCommand DeleteAllPointsCommand { get; set; }
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand EnterKeyCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand CopyCoordinateCommand { get; set; }
        public RelayCommand CopyAllCoordinatesCommand { get; set; }
        public RelayCommand PasteCoordinatesCommand { get; set; }

        private void OnDeletePointCommand(object obj)
        {
            var items = obj as IList;
            if (items == null)
                return;

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
        public override void OnCopyCommand(object obj)
        {
            var items = obj as IList;
            if (items == null)
                return;

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
            if (saveAsDialog.ShowDialog() == true)
            {
                IsToolActive = false;
                var fcUtils = new FeatureClassUtils();
                string path = fcUtils.PromptUserWithSaveDialog(vm.FeatureIsChecked, vm.ShapeIsChecked, vm.KmlIsChecked, vm.CSVIsChecked);
                if (path != null)
                {
                    var displayAmb = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
                    CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;
                    try
                    {
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        var mapPointList = CoordinateAddInPoints.Select(i => i.Point).ToList();
                        if (vm.FeatureIsChecked)
                        {
                            var ccMapPointList = GetMapPointExportFormat(CoordinateAddInPoints);
                            await fcUtils.CreateFCOutput(path,
                                                         SaveAsType.FileGDB,
                                                         ccMapPointList,
                                                         MapView.Active.Map.SpatialReference,
                                                         MapView.Active,
                                                         GeomType.Point);
                        }
                        else if (vm.ShapeIsChecked || vm.KmlIsChecked)
                        {
                            var ccMapPointList = GetMapPointExportFormat(CoordinateAddInPoints);
                            await fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, ccMapPointList, MapView.Active.Map.SpatialReference, MapView.Active, GeomType.Point, vm.KmlIsChecked);
                        }
                        else if (vm.CSVIsChecked)
                        {
                            var aiPoints = CoordinateAddInPoints.ToList();
                            if (!aiPoints.Any())
                                return;
                            var csvExport = new CsvExport();

                            foreach (var point in aiPoints)
                            {
                                var results = GetOutputFormats(point);
                                csvExport.AddRow();
                                foreach (var item in results)
                                    csvExport[item.Key] = item.Value;
                                if (point.FieldsDictionary != null)
                                {
                                    foreach (KeyValuePair<string, Tuple<object, bool>> item in point.FieldsDictionary)
                                        if (item.Key != PointFieldName && item.Key != OutputFieldName)
                                            csvExport[item.Key] = item.Value.Item1;
                                }
                            }
                            csvExport.ExportToFile(path);

                            System.Windows.Forms.MessageBox.Show(Properties.Resources.CSVExportSuccessfulMessage + path,
                                Properties.Resources.CSVExportSuccessfulCaption);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                    CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = displayAmb;
                }
            }
        }

        private List<CCProGraphic> GetMapPointExportFormat(ObservableCollection<AddInPoint> mapPointList)
        {
            List<CCProGraphic> results = new List<CCProGraphic>();
            var columnCollection = new List<string>();
            var dictionary = new Dictionary<string, object>();
            foreach (var point in mapPointList)
            {
                var attributes = GetOutputFormats(point);
                if (point.FieldsDictionary != null)
                {
                    foreach (KeyValuePair<string, Tuple<object, bool>> item in point.FieldsDictionary)
                        if (item.Key != PointFieldName && item.Key != OutputFieldName)
                        {
                            attributes[item.Key] = Convert.ToString(item.Value.Item1);
                            if (!columnCollection.Contains(item.Key))
                                columnCollection.Add(item.Key);
                        }
                }
                CCProGraphic ccMapPoint = new CCProGraphic() { Attributes = attributes, MapPoint = point.Point };
                results.Add(ccMapPoint);
            }
            foreach (var item in results)
            {
                foreach (var column in columnCollection)
                {
                    if (!item.Attributes.ContainsKey(column))
                    {
                        item.Attributes.Add(column, null);
                    }
                }
            }
            return results;
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

        private async void AddCollectionPoint(MapPoint point, Dictionary<string, Tuple<object, bool>> fieldsDictionary = null)
        {
            var guid = await AddGraphicToMap(point, ColorFactory.Instance.RedRGB, true, 7);
            var addInPoint = new AddInPoint() { Point = point, GUID = guid, FieldsDictionary = fieldsDictionary };

            //Add point to the top of the list
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
            RaisePropertyChanged(() => HasListBoxRightClickSelectedItem);
        }

        public override void OnPasteCommand(object obj)
        {
            var input = Clipboard.GetText().Trim();
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var coordinates = new List<string>();
            foreach (var item in lines)
            {
                if (item.Trim() == "")
                    continue;
                var sb = new StringBuilder();
                sb.Append(item.Trim());
                coordinates.Add(sb.ToString());
            }

            Mediator.NotifyColleagues(Constants.IMPORT_COORDINATES, coordinates);
        }

        #region overrides

        internal override void OnFlashPointCommandAsync(object obj)
        {
            if (MapView.Active == null)
            {
                System.Windows.Forms.MessageBox.Show(Properties.Resources.LoadMapMsg);
                return;
            }

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

            var input = obj as Dictionary<string, Tuple<object, bool>>;
            if (input != null)
            {
                var point = input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault();
                AddCollectionPoint(point as MapPoint, input);
            }
            else
            {
                var point = obj as MapPoint;
                AddCollectionPoint(point, input);
            }

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            // update list box coordinates
            var list = CoordinateAddInPoints.ToList();
            CoordinateAddInPoints.Clear();
            foreach (var item in list)
                CoordinateAddInPoints.Add(item);
        }

        public override async void OnMapPointSelection(object obj)
        {
            var mp = obj as MapPoint;
            var dblSrchDis = MapView.Active.Extent.Width / 200;
            var poly = GeometryEngine.Instance.Buffer(mp, dblSrchDis);
            AddInPoint closestPoint = null;
            Double distance = 0;
            foreach (var item in ProCollectTabViewModel.CoordinateAddInPoints)
            {
                if (item.Point.SpatialReference != MapView.Active.Map.SpatialReference)
                    item.Point = GeometryEngine.Instance.Project(item.Point, MapView.Active.Map.SpatialReference) as MapPoint;
                var isWithinExtent = await IsPointWithinExtent(item.Point, poly.Extent);
                if (isWithinExtent)
                {
                    double resultDistance = GeometryEngine.Instance.Distance(item.Point, mp);
                    distance = (distance < resultDistance && distance > 0) ? distance : resultDistance;

                    if (resultDistance == distance)
                    {
                        closestPoint = item;
                        distance = resultDistance;
                    }
                }
            }
            if (closestPoint != null)
            {
                closestPoint.IsSelected = true;
                ListBoxSelectedItem = closestPoint;
            }
            RaisePropertyChanged(() => ListBoxSelectedItem);
        }

        #endregion overrides

        internal async Task<bool> IsPointWithinExtent(MapPoint point, Envelope env)
        {
            var result = await QueuedTask.Run(() =>
            {
                Geometry projectedPoint = GeometryEngine.Instance.Project(point, env.SpatialReference);

                return GeometryEngine.Instance.Contains(env, projectedPoint);
            });

            return result;
        }
    }
}
