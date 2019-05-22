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
using System.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ProAppCoordConversionModule.Models;
using CoordinateConversionLibrary.Views;
using System.IO;
using System.Text;
using System.Linq;
using CoordinateConversionLibrary;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Reflection;
using ProAppCoordConversionModule.Views;
using System.Diagnostics;
using ProAppCoordConversionModule.Helpers;

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProTabBaseViewModel : TabBaseViewModel
    {
        // This name should correlate to the name specified in Config.esriaddinx - Tool id="ProAppCoordConversionModule_CoordinateMapTool"
        internal const string MapPointToolName = "ProAppCoordConversionModule_CoordinateMapTool";

        public ProTabBaseViewModel()
        {
            ActivatePointToolCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnFlashPointCommandAsync);
            ViewDetailCommand = new CoordinateConversionLibrary.Helpers.RelayCommand(OnViewDetailCommand);
            FieldsCollection = new ObservableCollection<FieldsCollection>();
            ViewDetailsTitle = string.Empty;
            ListDictionary = new List<Dictionary<string, Tuple<object, bool>>>();
            Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            Mediator.Register("FLASH_COMPLETED", OnFlashCompleted);
            pDialog = new ProgressDialog("Processing...Please wait...");
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetCoordinateGetter, proCoordGetter);

            ArcGIS.Desktop.Framework.Events.ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
        }

        public CoordinateConversionLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateConversionLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }
        public CoordinateConversionLibrary.Helpers.RelayCommand ViewDetailCommand { get; set; }

        public static ProgressDialog pDialog { get; set; }
        public static ProCoordinateGet proCoordGetter = new ProCoordinateGet();
        public String PreviousTool { get; set; }
        public static ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }
        public ObservableCollection<FieldsCollection> FieldsCollection { get; set; }
        public string ViewDetailsTitle { get; set; }
        public static Dictionary<string, ObservableCollection<Symbol>> AllSymbolCollection { get; set; }
        public ProAdditionalFieldsView DialogView { get; set; }
        public bool IsDialogViewOpen { get; set; }

        public static Symbol SelectedSymbolObject { get; set; }
        public static PropertyInfo SelectedColorObject { get; set; }

        public static bool Is3DMap { get; set; }

        public static Symbol SelectedSymbol { get; set; }

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
                CoordinateMapTool.SelectFeatureEnable = false;
                isToolActive = value;
                ActivateMapTool(value);

                RaisePropertyChanged(() => IsToolActive);
            }
        }

        public bool isSelectionToolActive = false;
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
                CoordinateMapTool.SelectFeatureEnable = true;
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
                string currentTool = FrameworkApplication.CurrentTool;
                if (currentTool != MapPointToolName)
                {
                    // Save previous tool to reactivate
                    PreviousTool = currentTool;
                    toolToActivate = MapPointToolName;
                }
            }
            else
            {
                // Handle case if no Previous Tool
                if (string.IsNullOrEmpty(PreviousTool))
                    PreviousTool = "esri_mapping_exploreTool";

                toolToActivate = PreviousTool;
            }

            if (!string.IsNullOrEmpty(toolToActivate))
            {
                FrameworkApplication.SetCurrentToolAsync(toolToActivate);
            }
        }

        private static System.IDisposable _overlayObject = null;

        /// <summary>
        /// lists to store GUIDs of graphics, temp feedback and map graphics
        /// </summary>
        public static List<ProGraphic> ProGraphicsList = new List<ProGraphic>();

        private void ClearOverlay()
        {
            if (_overlayObject != null)
            {
                _overlayObject.Dispose();
                _overlayObject = null;
            }
        }

        #region overrides

        public override bool OnNewMapPoint(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;
            var input = obj as Dictionary<string, Tuple<object, bool>>;
            MapPoint mp = (input != null) ? input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault() as MapPoint : obj as MapPoint;
            if (mp == null)
                return false;

            proCoordGetter.Point = mp;
            InputCoordinate = proCoordGetter.GetInputDisplayString();

            return true;
        }

        public override void OnValidateMapPoint(object obj)
        {
            if (OnValidationSuccess(obj))
            {
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NEW_MAP_POINT, obj);
            }
        }

        public bool OnValidationSuccess(object obj)
        {
            if (!base.OnNewMapPoint(obj))
                return false;
            var input = obj as Dictionary<string, Tuple<object, bool>>;
            MapPoint mp = (input != null) ? input.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault() as MapPoint : obj as MapPoint;
            if (mp == null)
                return false;

            var isValidPoint = QueuedTask.Run(async () => { return await IsValidPoint(mp); });
            if (!isValidPoint.Result)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Point is out of bounds", "Point is out of bounds",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public override bool OnMouseMove(object obj)
        {
            if (!base.OnMouseMove(obj))
                return false;

            var mp = obj as MapPoint;

            if (mp == null)
                return false;

            proCoordGetter.Point = mp;
            InputCoordinate = proCoordGetter.GetInputDisplayString();

            return true;
        }

        public override void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            base.OnDisplayCoordinateTypeChanged(obj);

            if (proCoordGetter != null && proCoordGetter.Point != null)
            {
                InputCoordinate = proCoordGetter.GetInputDisplayString();
            }
        }

        private string processCoordinate(CCCoordinate ccc)
        {
            if (ccc == null)
                return string.Empty;

            HasInputError = false;
            string result = string.Empty;

            if (ccc.Type == CoordinateType.Unknown)
            {
                HasInputError = true;
                proCoordGetter.Point = null;
                foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    output.OutputCoordinate = "";
                    output.Props.Clear();
                }
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.InvalidCoordMsg,
                    CoordinateConversionLibrary.Properties.Resources.InvalidCoordCap);
            }
            else
            {
                proCoordGetter.Point = ccc.Point;
                result = new CoordinateDD(ccc.Point.Y, ccc.Point.X).ToString("", new CoordinateDDFormatter());
                if (CoordinateBase.InputFormatSelection == CoordinateTypes.Custom.ToString())
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                else
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
            }

            return result;
        }

        public override string ProcessInput(string input)
        {
            if (input == "NA") return string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            // Must force non async here to avoid returning to base class early
            var ccc = QueuedTask.Run(() =>
            {
                return GetCoordinateType(input);
            }).Result;
            return processCoordinate(ccc);
        }

        public async Task<string> ProcessInputAsync(string input)
        {
            if (input == "NA") return string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var ccc = await GetCoordinateType(input);

            return processCoordinate(ccc);
        }

        public Dictionary<string, string> GetOutputFormats(AddInPoint point)
        {
            var results = new Dictionary<string, string>();
            results.Add(CoordinateFieldName, point.Text);
            var ccc = QueuedTask.Run(() =>
            {
                return GetCoordinateType(point.Text);
            }).Result;
            if (ccc != null && ccc.Point != null)
            {
                ProCoordinateGet procoordinateGetter = new ProCoordinateGet();
                procoordinateGetter.Point = ccc.Point;
                CoordinateGetBase coordinateGetter = procoordinateGetter as CoordinateGetBase;
                foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    var props = new Dictionary<string, string>();
                    string coord = string.Empty;

                    switch (output.CType)
                    {
                        case CoordinateType.DD:
                            CoordinateDD cdd;
                            if (coordinateGetter.CanGetDD(output.SRFactoryCode, out coord) &&
                                CoordinateDD.TryParse(coord, out cdd, true))
                            {
                                results.Add(output.Name, cdd.ToString(output.Format, new CoordinateDDFormatter()));
                            }
                            break;
                        case CoordinateType.DMS:
                            CoordinateDMS cdms;
                            if (coordinateGetter.CanGetDMS(output.SRFactoryCode, out coord) &&
                                CoordinateDMS.TryParse(coord, out cdms, true))
                            {
                                results.Add(output.Name, cdms.ToString(output.Format, new CoordinateDMSFormatter()));
                            }
                            break;
                        case CoordinateType.DDM:
                            CoordinateDDM ddm;
                            if (coordinateGetter.CanGetDDM(output.SRFactoryCode, out coord) &&
                                CoordinateDDM.TryParse(coord, out ddm, true))
                            {
                                results.Add(output.Name, ddm.ToString(output.Format, new CoordinateDDMFormatter()));
                            }
                            break;
                        case CoordinateType.GARS:
                            CoordinateGARS gars;
                            if (coordinateGetter.CanGetGARS(output.SRFactoryCode, out coord) &&
                                CoordinateGARS.TryParse(coord, out gars))
                            {
                                results.Add(output.Name, gars.ToString(output.Format, new CoordinateGARSFormatter()));
                            }
                            break;
                        case CoordinateType.MGRS:
                            CoordinateMGRS mgrs;
                            if (coordinateGetter.CanGetMGRS(output.SRFactoryCode, out coord) &&
                                CoordinateMGRS.TryParse(coord, out mgrs))
                            {
                                results.Add(output.Name, mgrs.ToString(output.Format, new CoordinateMGRSFormatter()));
                            }
                            break;
                        case CoordinateType.USNG:
                            CoordinateUSNG usng;
                            if (coordinateGetter.CanGetUSNG(output.SRFactoryCode, out coord) &&
                                CoordinateUSNG.TryParse(coord, out usng))
                            {
                                results.Add(output.Name, usng.ToString(output.Format, new CoordinateMGRSFormatter()));
                            }
                            break;
                        case CoordinateType.UTM:
                            CoordinateUTM utm;
                            if (coordinateGetter.CanGetUTM(output.SRFactoryCode, out coord) &&
                                CoordinateUTM.TryParse(coord, out utm))
                            {
                                results.Add(output.Name, utm.ToString(output.Format, new CoordinateUTMFormatter()));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return results;
        }

        public override void OnEditPropertiesDialogCommand(object obj)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
            {
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.LoadMapMsg);
                return;
            }

            var dlg = new ProEditPropertiesView();
            dlg.DataContext = new ProEditPropertiesViewModel();
            try
            {
                dlg.ShowDialog();
            }
            catch (Exception e)
            {
                if (e.Message.ToLower() == CoordinateConversionLibrary.Properties.Resources.CoordsOutOfBoundsMsg.ToLower())
                {
                    System.Windows.Forms.MessageBox.Show(e.Message + System.Environment.NewLine + CoordinateConversionLibrary.Properties.Resources.CoordsOutOfBoundsAddlMsg,
                        CoordinateConversionLibrary.Properties.Resources.CoordsoutOfBoundsCaption);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
        }

        public bool IsView3D()
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
                return false;

            //Return whether the viewing mode is SceneLocal or SceneGlobal
            return mapView.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.SceneLocal ||
                   mapView.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.SceneGlobal;
        }

        public override void OnImportCSVFileCommand(object obj)
        {
            try
            {
                CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;
                var fileDialog = new Microsoft.Win32.OpenFileDialog();
                fileDialog.CheckFileExists = true;
                fileDialog.CheckPathExists = true;
                fileDialog.Filter = "csv files|*.csv|Excel 97-2003 Workbook (*.xls)|*.xls|Excel Workbook (*.xlsx)|*.xlsx";
                // attemp to import
                var fieldVM = new SelectCoordinateFieldsViewModel();
                var result = fileDialog.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    var dlg = new ProSelectCoordinateFieldsView();

                    var coordinates = new List<string>();
                    var extension = Path.GetExtension(fileDialog.FileName);
                    switch (extension)
                    {
                        case ".csv":
                            ImportFromCSV(fieldVM, dlg, coordinates, fileDialog.FileName);
                            break;
                        case ".xls":
                        case ".xlsx":
                            ImportFromExcel(dlg, fileDialog, fieldVM);
                            break;
                        default:
                            break;
                    }
                }
                CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = true;
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Something went wrong.");
                Debug.WriteLine("Error " + ex.ToString());
            }
        }

        private static void ImportFromCSV(SelectCoordinateFieldsViewModel fieldVM, ProSelectCoordinateFieldsView dlg, List<string> coordinates, string fileName)
        {
            using (Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var headers = ImportCSV.GetHeaders(s);
                ImportedData = new List<Dictionary<string, Tuple<object, bool>>>();
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        fieldVM.AvailableFields.Add(header);
                        fieldVM.FieldCollection.Add(new ListBoxItem() { Name = header, Content = header, IsSelected = false });
                        System.Diagnostics.Debug.WriteLine("header : {0}", header);
                    }
                    dlg.DataContext = fieldVM;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.MsgNoDataFound);
                    return;
                }
                if (dlg.ShowDialog() == true)
                {
                    var dictionary = new List<Dictionary<string, Tuple<object, bool>>>();

                    using (Stream str = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var lists = ImportCSV.Import<ImportCoordinatesList>(str, fieldVM.SelectedFields.ToArray(), headers, dictionary);
                        FieldCollection = fieldVM.FieldCollection.Where(y => y.IsSelected).Select(x => x.Content).ToList();
                        foreach (var item in dictionary)
                        {
                            var dict = new Dictionary<string, Tuple<object, bool>>();
                            foreach (var field in item)
                            {
                                if (FieldCollection.Contains(field.Key))
                                    dict.Add(field.Key, Tuple.Create((object)field.Value.Item1, FieldCollection.Contains(field.Key)));
                                if (fieldVM.SelectedFields.ToArray()[0] == field.Key)
                                    SelectedField1 = Convert.ToString(field.Key);
                                else if (fieldVM.UseTwoFields)
                                    if (fieldVM.SelectedFields.ToArray()[1] == field.Key)
                                        SelectedField2 = Convert.ToString(field.Key);
                            }
                            var lat = item.Where(x => x.Key == fieldVM.SelectedFields.ToArray()[0]).Select(x => x.Value.Item1).FirstOrDefault();
                            var sb = new StringBuilder();
                            sb.Append(lat);
                            if (fieldVM.UseTwoFields)
                            {
                                var lon = item.Where(x => x.Key == fieldVM.SelectedFields.ToArray()[1]).Select(x => x.Value.Item1).FirstOrDefault();
                                sb.Append(string.Format(" {0}", lon));
                            }
                            dict.Add(OutputFieldName, Tuple.Create((object)sb.ToString(), false));
                            ImportedData.Add(dict);
                        }
                    }
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, ImportedData);
                }
            }
        }

        public static async void ImportFromExcel(ProSelectCoordinateFieldsView dlg, Microsoft.Win32.OpenFileDialog diag, SelectCoordinateFieldsViewModel fieldVM)
        {
            ImportedData = new List<Dictionary<string, Tuple<object, bool>>>();
            List<string> headers = new List<string>();
            var filename = diag.FileName;
            string selectedCol1Key = "", selectedCol2Key = "";
            var selectedColumn = fieldVM.SelectedFields.ToArray();
            var columnCollection = new List<string>();
            var fileExt = Path.GetExtension(filename); //get the extension of uploaded excel file  
            var lstDictionary = await FeatureClassUtils.ImportFromExcel(filename);
            var headerDict = lstDictionary.FirstOrDefault();
            if (headerDict != null)
                foreach (var item in headerDict)
                {
                    if (item.Key != "OBJECTID")
                        headers.Add(item.Key);
                }
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    fieldVM.AvailableFields.Add(header.Replace(" ", ""));
                    fieldVM.FieldCollection.Add(new ListBoxItem() { Name = header.Replace(" ", ""), Content = header, IsSelected = false });
                }
                dlg.DataContext = fieldVM;
            }
            else
                System.Windows.Forms.MessageBox.Show(CoordinateConversionLibrary.Properties.Resources.MsgNoDataFound);
            if (dlg.ShowDialog() == true)
            {
                foreach (var item in headers)
                {
                    columnCollection.Add(item);
                    if (item == fieldVM.SelectedFields.ToArray()[0])
                    {
                        selectedCol1Key = item;
                        SelectedField1 = item;
                    }
                    if (fieldVM.UseTwoFields && item == fieldVM.SelectedFields.ToArray()[1])
                    {
                        selectedCol2Key = item;
                        SelectedField2 = item;
                    }
                }
                for (int i = 0; i < lstDictionary.Count; i++)
                {
                    var dict = new Dictionary<string, Tuple<object, bool>>();
                    dict = lstDictionary[i];
                    if (fieldVM.UseTwoFields)
                    {
                        if (lstDictionary[i].Where(x => x.Key == selectedCol1Key) != null && lstDictionary[i].Where(x => x.Key == selectedCol2Key) != null)
                            dict.Add(OutputFieldName, Tuple.Create((object)Convert.ToString(lstDictionary[i].Where(x => x.Key == selectedCol1Key).Select(x => x.Value.Item1).FirstOrDefault()
                                + " " + lstDictionary[i].Where(x => x.Key == selectedCol2Key).Select(x => x.Value.Item1).FirstOrDefault()), false));
                    }
                    else
                    {
                        if (lstDictionary[i].Where(x => x.Key == selectedCol1Key) != null)
                            dict.Add(OutputFieldName, Tuple.Create((object)lstDictionary[i].Where(x => x.Key == selectedCol1Key).Select(x => x.Value.Item1).FirstOrDefault(), false));
                    }
                    ImportedData.Add(dict);
                }
                FieldCollection = fieldVM.FieldCollection.Where(y => y.IsSelected).Select(x => x.Content).ToList();
                foreach (var item in ImportedData)
                {
                    var dict = new Dictionary<string, Tuple<object, bool>>();
                    foreach (var field in item)
                    {
                        if (FieldCollection.Contains(field.Key) || field.Key == OutputFieldName)
                            dict.Add(field.Key, Tuple.Create(field.Value.Item1, FieldCollection.Contains(field.Key)));
                    }
                    ListDictionary.Add(dict);
                }
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, ListDictionary);
            }
        }

        #endregion overrides

        #region Mediator handlers

        private void OnBCNeeded(object obj)
        {
            if (proCoordGetter == null || proCoordGetter.Point == null)
                return;

            BroadcastCoordinateValues(proCoordGetter.Point);
        }

        private void OnFlashCompleted(object obj)
        {
            IsToolActive = false;
            CoordinateMapTool.AllowUpdates = true;
        }

        #endregion Mediator handlers

        private void SetAsCurrentToolAsync()
        {
            IsToolActive = true;
        }

        private void OnMapToolCommand(object obj)
        {
            SetAsCurrentToolAsync();
        }

        private void OnActiveToolChanged(ArcGIS.Desktop.Framework.Events.ToolEventArgs args)
        {
            // Update active tool when tool changed so Map Point Tool button push state
            // stays in sync with Pro UI
            if (IsSelectionToolActive)
            {
                IsSelectionToolActive = args.CurrentID == MapPointToolName;
                RaisePropertyChanged(() => IsSelectionToolActive);
            }
            else
            {
                isToolActive = args.CurrentID == MapPointToolName;
                RaisePropertyChanged(() => IsToolActive);
            }
        }

        internal async Task<string> AddGraphicToMap(Geometry geom, CIMColor color, bool IsTempGraphic = false, double size = 1.0, string text = "", SimpleMarkerStyle markerStyle = SimpleMarkerStyle.Circle, string tag = "")
        {
            if (geom == null || MapView.Active == null)
                return string.Empty;

            CIMSymbolReference symbol = null;

            if (!string.IsNullOrWhiteSpace(text) && geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                {
                    // TODO add text graphic
                    //var tg = new CIMTextGraphic() { Placement = Anchor.CenterPoint, Text = text};
                });
            }
            else if (geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                {
                    var s = SymbolFactory.Instance.ConstructPointSymbol(color, size, markerStyle);
                    var haloSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreenRGB);
                    haloSymbol.SetOutlineColor(ColorFactory.Instance.GreenRGB);
                    s.HaloSymbol = haloSymbol;
                    s.HaloSize = 0;
                    if (SelectedSymbol != null)
                    {
                        if (Is3DMap)
                        {
                            var symbol3D = ((ArcGIS.Core.CIM.CIMPointSymbol)(SelectedSymbol.SymbolItem.Symbol));
                            symbol3D.UseRealWorldSymbolSizes = false;
                            symbol = new CIMSymbolReference() { Symbol = symbol3D };
                        }
                        else
                        {
                            symbol = new CIMSymbolReference() { Symbol = SelectedSymbol.SymbolItem.Symbol };
                        }

                    }
                    else
                        symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                {
                    var s = SymbolFactory.Instance.ConstructLineSymbol(color, size);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }
            else if (geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.Instance.ConstructPolygonSymbol(color, SimpleFillStyle.Solid, outline);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }

            var result = await QueuedTask.Run(() =>
            {
                var disposable = MapView.Active.AddOverlay(geom, symbol);
                var guid = Guid.NewGuid().ToString();
                ProGraphicsList.Add(new ProGraphic(disposable, guid, geom, symbol, IsTempGraphic, tag));
                return guid;
            });

            return result;
        }

        internal async virtual void OnFlashPointCommandAsync(object obj)
        {
            var point = obj as MapPoint;

            if (point == null)
                return;

            IsToolActive = true;

            await QueuedTask.Run(() =>
            {
                // zoom to point
                var projectedPoint = GeometryEngine.Instance.Project(point, MapView.Active.Extent.SpatialReference);

                // WORKAROUND: delay zoom by 1 sec to give Map Point Tool enough time to activate
                // Note: The Map Point Tool is required to be active to enable flash overlay
                MapView.Active.PanTo(projectedPoint, new TimeSpan(0, 0, 1));

                Mediator.NotifyColleagues("UPDATE_FLASH", point);
            });
        }

        internal virtual void OnViewDetailCommand(object obj)
        {
            var input = obj as System.Windows.Controls.ListBox;
            if (input.SelectedItems.Count == 0)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No data available");
                return;
            }
            ShowPopUp(((input.SelectedItems)[0] as AddInPoint));
        }

        private void ShowPopUp(AddInPoint addinPoint)
        {
            var dictionary = addinPoint.FieldsDictionary;
            var htmlString = "<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                                + "<meta charset=\"utf-8\">"
                                + "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\">"
                                + "<title>Popup</title>"
                                + "<link rel=\"Stylesheet\" type=\"text/css\" href=\"c:/program files/arcgis/pro/Resources/Popups/esri/css/Popups.css\">"
                                + "</head>"
                                + "<body>"
                                + "<div class=\"esriPopup\">"
                                + "<div class=\"esriPopupWrapper\">"
                                + "<div class=\"sizer content\">"
                                + "<div class=\"contentPane\">"
                                + "<div class=\"esriViewPopup\">"
                                + "<div class=\"mainSection\">"
                                + "<div><!--POPUP_MAIN_CONTENT_TEXT--></div>"
                                + "<div><table class=\"attrTable\" cellspacing=\"0\" cellpadding=\"0\"><tbody>";
            FieldsCollection = new ObservableCollection<FieldsCollection>();
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    htmlString = htmlString + "<tr valign=\"top\">";
                    if (item.Value.Item2)
                    {
                        htmlString = htmlString + "<td class=\"attrName\">" + item.Key + "</td><td class=\"attrValue\">" + Convert.ToString(item.Value.Item1) + "</td>";

                        FieldsCollection.Add(new FieldsCollection() { FieldName = item.Key, FieldValue = Convert.ToString(item.Value.Item1) });
                    }
                    htmlString = htmlString + "</tr>";
                }
                var valOutput = dictionary.Where(x => x.Key == PointFieldName).Select(x => x.Value.Item1).FirstOrDefault();
                ViewDetailsTitle = MapPointHelper.GetMapPointAsDisplayString(valOutput as MapPoint);
            }
            else
                ViewDetailsTitle = addinPoint.Text;

            if (!FieldsCollection.Any())
            {
                htmlString = htmlString + "<tr valign=\"top\"><td class=\"attrName\">"
                    + CoordinateConversionLibrary.Properties.Resources.InformationNotAvailableMsg + "</td></tr>";
            }
            htmlString = htmlString + "</tbody></table></div>"
                                        + "</div>"
                                        + "<div id=\"mediaSection\" class=\"mediaSection hidden\">"
                                        + "<div id=\"mediaTitle\" class=\"header\"></div>"
                                        + "<div id=\"mediaTitleLine\" class=\"hzLine\"></div>"
                                        + "<div id=\"mediaDescription\" class=\"caption\"></div>"
                                        + "<div id=\"gallery\" class=\"gallery\">"
                                        + "<div id=\"prevMedia\" title=\"Previous media\" class=\"mediaHandle prev\"></div>"
                                        + "<div id=\"nextMedia\" title=\"Next media\" class=\"mediaHandle next\"></div>"
                                        + "<ul id=\"mediaSummary\" class=\"summary\">"
                                        + "<li id=\"imageCount\" class=\"image mediaCount\">0</li>"
                                        + "<li id=\"imageIcon\" class=\"image mediaIcon\"></li>"
                                        + "<li id=\"chartCount\" class=\"chart mediaCount\">0</li>"
                                        + "<li id=\"chartIcon\" class=\"chart mediaIcon\"></li>"
                                        + "</ul>"
                                        + "<div id=\"mediaFrame\" class=\"frame\" style=\"-ms-user-select: none;\">"
                                        + "<div id=\"mediaTarget\" class=\"chart\"></div>"
                                        + "</div>"
                                        + "</div>"
                                        + "<br><br>"
                                        + "</div>"
                                        + "<div>"
                                        + "<div><!--POPUP_ATTACHMENTS--></div>"
                                        + "</div>"
                                        + "</div>"
                                        + "</div>"
                                        + "</div>"
                                        + "</div>"
                                        + "</div>"
                                        + "<script type=\"text/javascript\" src=\"c:/program files/arcgis/pro/Resources/Popups/dojo/dojo.js\"></script>"
                                        + "<script type=\"text/javascript\" src=\"c:/program files/arcgis/pro/Resources/Popups/esri/run.js\"></script>"
                                        + "</body></html>";



            MapView.Active.ShowCustomPopup(new List<PopupContent>() {
                new PopupContent(htmlString,ViewDetailsTitle)
            });
        }

        private void diagView_Closed(object sender, EventArgs e)
        {
            IsDialogViewOpen = false;
        }

        internal async void UpdateHighlightedGraphics(bool reset, bool isUpdateAll = false)
        {
            var list = ProGraphicsList.ToList();
            foreach (var proGraphic in list)
            {
                var aiPoint = CoordinateAddInPoints.FirstOrDefault(p => p.GUID == proGraphic.GUID);
                if (aiPoint != null)
                {
                    var s = proGraphic.SymbolRef.Symbol as CIMPointSymbol;

                    var doUpdate = false;

                    if (s == null)
                        continue;

                    if (aiPoint.IsSelected)
                    {
                        if (reset)
                        {
                            s.HaloSize = 0;
                            aiPoint.IsSelected = false;
                        }
                        else
                            s.HaloSize = 2;
                        doUpdate = true;
                    }
                    else if (s.HaloSize > 0)
                    {
                        s.HaloSize = 0;
                        doUpdate = true;
                    }

                    if (doUpdate || isUpdateAll)
                    {
                        var result = await QueuedTask.Run(() =>
                        {
                            if (SelectedSymbolObject != null)
                            {
                                if (Is3DMap)
                                {
                                    var symbol3D = ((ArcGIS.Core.CIM.CIMPointSymbol)(SelectedSymbol.SymbolItem.Symbol));
                                    symbol3D.UseRealWorldSymbolSizes = false;
                                    proGraphic.SymbolRef.Symbol = symbol3D;
                                }
                                else
                                {
                                    proGraphic.SymbolRef.Symbol = SelectedSymbolObject.SymbolItem.Symbol;
                                }
                                var symbol = proGraphic.SymbolRef.Symbol as CIMPointSymbol;
                                if (aiPoint.IsSelected)
                                {
                                    symbol.HaloSize = 2;
                                    var haloSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreenRGB);
                                    haloSymbol.SetOutlineColor(ColorFactory.Instance.GreenRGB);
                                    symbol.HaloSymbol = haloSymbol;
                                }
                            }
                            var temp = MapView.Active.UpdateOverlay(proGraphic.Disposable, proGraphic.Geometry, proGraphic.SymbolRef);
                            return temp;
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Method to check to see point is withing the map area of interest
        /// </summary>
        /// <param name="point">IPoint to validate</param>
        /// <returns></returns>
        internal async Task<bool> IsValidPoint(MapPoint point)
        {
            if ((point != null) && (MapView.Active != null) && (MapView.Active.Map != null))
            {
                Envelope env = null;
                await QueuedTask.Run(() =>
                {
                    env = MapView.Active.Map.CalculateFullExtent();
                });

                bool isValid = false;

                if (env != null)
                {
                    if (env.SpatialReference != point.SpatialReference)
                    {
                        point = GeometryEngine.Instance.Project(point, env.SpatialReference) as MapPoint;
                    }
                    isValid = GeometryEngine.Instance.Contains(env, point);
                }

                return isValid;
            }
            return false;
        }

        #region Private Methods

        private void BroadcastCoordinateValues(MapPoint mapPoint)
        {
            var dict = new Dictionary<CoordinateType, string>();
            if (mapPoint == null)
                return;

            var dd = new CoordinateDD(mapPoint.Y, mapPoint.X);

            try
            {
                dict.Add(CoordinateType.DD, dd.ToString("", new CoordinateDDFormatter()));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.DDM, new CoordinateDDM(dd).ToString("", new CoordinateDDMFormatter()));
            }
            catch { /* Conversion Failed */ }
            try
            {
                dict.Add(CoordinateType.DMS, new CoordinateDMS(dd).ToString("", new CoordinateDMSFormatter()));
            }
            catch { /* Conversion Failed */ }

            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);
        }

        private CoordinateType GetCoordinateType(string input, out MapPoint point)
        {
            point = null;

            // DD
            CoordinateDD dd;
            if (CoordinateDD.TryParse(input, out dd, true))
            {
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DD;
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm, true))
            {
                dd = new CoordinateDD(ddm);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DDM;
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms, true))
            {
                dd = new CoordinateDD(dms);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DMS;
            }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(gars, GeoCoordinateType.GARS);
                    }).Result;

                    return CoordinateType.GARS;
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(mgrs, GeoCoordinateType.MGRS);
                    }).Result;

                    return CoordinateType.MGRS;
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(usng, GeoCoordinateType.USNG);
                    }).Result;

                    return CoordinateType.USNG;
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(utm, GeoCoordinateType.UTM);
                    }).Result;

                    return CoordinateType.UTM;
                }
                catch { /* Conversion Failed */ }
            }

            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]*(?<longitude>\-?\d+[.,]?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
                    point = QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
                    }).Result;
                    return CoordinateType.DD;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to convert coordinate: " + ex.Message);
                    return CoordinateType.Unknown;
                }
            }

            return CoordinateType.Unknown;
        }
        private async Task<CCCoordinate> GetCoordinateType(string input)
        {
            MapPoint point = null;

            // DD
            CoordinateDD dd;
            CoordinateDD.ShowAmbiguousEventHandler += ShowAmbiguousEventHandler;
            if (CoordinateDD.TryParse(input, out dd, true))
            {
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
            }

            // DDM
            CoordinateDDM ddm;
            if (CoordinateDDM.TryParse(input, out ddm, true))
            {
                dd = new CoordinateDD(ddm);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DDM, Point = point };
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms, true))
            {
                dd = new CoordinateDD(dms);
                if (dd.Lat > 90 || dd.Lat < -90 || dd.Lon > 180 || dd.Lon < -180)
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                point = await QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                });//.Result;
                return new CCCoordinate() { Type = CoordinateType.DMS, Point = point };
            }

            CoordinateGARS gars;
            if (CoordinateGARS.TryParse(input, out gars))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(gars, GeoCoordinateType.GARS);
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.GARS, Point = point };
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateMGRS mgrs;
            if (CoordinateMGRS.TryParse(input, out mgrs))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(mgrs, GeoCoordinateType.MGRS);
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.MGRS, Point = point };
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateUSNG usng;
            if (CoordinateUSNG.TryParse(input, out usng))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(usng, GeoCoordinateType.USNG);
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.USNG, Point = point }; ;
                }
                catch { /* Conversion Failed */ }
            }

            CoordinateUTM utm;
            if (CoordinateUTM.TryParse(input, out utm))
            {
                try
                {
                    point = await QueuedTask.Run(() =>
                    {
                        return convertToMapPoint(utm, GeoCoordinateType.UTM);
                    });//.Result;

                    return new CCCoordinate() { Type = CoordinateType.UTM, Point = point };
                }
                catch { /* Conversion Failed */ }
            }

            /*
             * Updated RegEx to capture invalid coordinates like 00, 45, or 456987. 
             */
            Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+[.,]?\d*)[+,;:\s]{1,}(?<longitude>\-?\d+[.,]?\d*)");

            var matchMercator = regexMercator.Match(input);

            if (matchMercator.Success && matchMercator.Length == input.Length)
            {
                try
                {
                    var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                    var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                    var sr = proCoordGetter.Point != null ? proCoordGetter.Point.SpatialReference : SpatialReferences.WebMercator;
                    point = await QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.CreateMapPoint(Lon, Lat, sr);
                    });//.Result;
                    return new CCCoordinate() { Type = CoordinateType.DD, Point = point };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to convert coordinate: " + ex.Message);
                    return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
                }
            }

            return new CCCoordinate() { Type = CoordinateType.Unknown, Point = null };
        }

        /// <summary>
        /// Helper function to convert input coordinate to display coordinate. For example, input 
        /// coordinate can be in MGRS and display coordinate is in DD. This function helps with that 
        /// conversion
        /// </summary>
        /// <param name="cb">Coordinate notation type</param>
        /// <param name="fromCoordinateType">Input coordinate notation type</param>
        /// <returns></returns>
        private MapPoint convertToMapPoint(CoordinateBase cb, GeoCoordinateType fromCoordinateType)
        {
            MapPoint retMapPoint = null;
            //Create WGS84 SR
            SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
            var coordType = GeoCoordinateType.DD;
            var succeed = Enum.TryParse(CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType.ToString(), out coordType);
            if (succeed)
            {
                try
                {
                    //Create map point from coordinate string
                    var fromCoord = MapPointBuilder.FromGeoCoordinateString(cb.ToString(), sptlRef, fromCoordinateType);
                    var geoCoordParam = new ToGeoCoordinateParameter(coordType);
                    var geoStr = fromCoord.ToGeoCoordinateString(geoCoordParam);
                    //Convert to map point with correct coordinate notation
                    retMapPoint = MapPointBuilder.FromGeoCoordinateString(geoStr, sptlRef, coordType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                IFormatProvider coordinateFormatter = null;
                switch (fromCoordinateType)
                {
                    case GeoCoordinateType.GARS:
                        coordinateFormatter = new CoordinateGARSFormatter();
                        break;
                    case GeoCoordinateType.MGRS:
                        coordinateFormatter = new CoordinateMGRSFormatter();
                        break;
                    case GeoCoordinateType.USNG:
                        coordinateFormatter = new CoordinateMGRSFormatter();
                        break;
                    case GeoCoordinateType.UTM:
                        coordinateFormatter = new CoordinateUTMFormatter();
                        break;
                    default:
                        Console.WriteLine("Unable to determine coordinate type");
                        break;
                };
                retMapPoint = MapPointBuilder.FromGeoCoordinateString(cb.ToString("", coordinateFormatter), sptlRef, fromCoordinateType, FromGeoCoordinateMode.Default);
            }
            return retMapPoint;
        }

        #endregion Private Methods

        #region Public Static Methods
        public static void ShowAmbiguousEventHandler(object sender, AmbiguousEventArgs e)
        {
            if (e.IsEventHandled)
            {
                var ambiguous = new ProAmbiguousCoordsView();
                ambiguous.DataContext = new ProAmbiguousCoordsViewModel();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ambiguous.ShowDialog();
                });
                e.IsEventHandled = false;
            }
        }
        #endregion

    }

    public class CCCoordinate
    {
        public CCCoordinate() { }
        public CoordinateType Type { get; set; }
        public MapPoint Point { get; set; }
        public CoordinateBase PointInformation { get; set; }
    }
}
