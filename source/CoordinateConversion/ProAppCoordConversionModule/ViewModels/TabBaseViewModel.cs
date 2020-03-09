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
using ProAppCoordConversionModule.Helpers;
using ProAppCoordConversionModule.Models;
using ProAppCoordConversionModule.Views;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Diagnostics;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;

namespace ProAppCoordConversionModule.ViewModels
{
    public class TabBaseViewModel : ViewModelBase
    {
        public TabBaseViewModel()
        {
            HasInputError = false;
            IsHistoryUpdate = true;
            IsToolGenerated = false;

            // commands
            EditPropertiesDialogCommand = new RelayCommand(OnEditPropertiesDialogCommand);
            ImportCSVFileCommand = new RelayCommand(OnImportCSVFileCommand);
            CopyCommand = new RelayCommand(OnCopyCommand);
            PasteCommand = new RelayCommand(OnPasteCommand);

            ListDictionary = new List<Dictionary<string, Tuple<object, bool>>>();
            FieldCollection = new List<object>();

            //Anupam
            NewMapPointInternal = new RelayCommand(OnNewMapPointInternal);
            ValidateMapPointInternal = new RelayCommand(OnValidateMapPointInternal);
            MouseMoveInternal = new RelayCommand(OnMouseMoveInternal);
            SelectMapPointInternal = new RelayCommand(OnSelectMapPointInternal);
         
      
            configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
                .RegisterHandler(n => n.DisplayCoordinateType, OnDisplayCoordinateTypeChanged);
        }        

        PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        public RelayCommand EditPropertiesDialogCommand { get; set; }
        public RelayCommand ImportCSVFileCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand PasteCommand { get; set; }
        public RelayCommand NewMapPointInternal { get; set; }
        public RelayCommand ValidateMapPointInternal { get; set; }
        public RelayCommand SelectMapPointInternal { get; set; }
        public RelayCommand MouseMoveInternal { get; set; }


        //TODO is this used anymore?
        public static string OutputFieldName = "OutputCoordinate";
        public static string PointFieldName = "Point";
        public static string CoordinateFieldName = "Coordinate";
        public static string SelectedField1 = "";
        public static string SelectedField2 = "";
        public static List<object> FieldCollection { get; set; }
        public static List<Dictionary<string, Tuple<object, bool>>> ImportedData { get; set; }
        public static List<Dictionary<string, Tuple<object, bool>>> ListDictionary { get; set; }
        public ObservableCollection<ListBoxItem> SelectedFieldItem { get; set; }
        public bool IsHistoryUpdate { get; set; }

        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                NotifyPropertyChanged(() => HasInputError);
            }
        }

        private string _inputCoordinate;
        public string InputCoordinate
        {
            get
            {
                return _inputCoordinate;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                _inputCoordinate = value;

                // DJH - Removed the following to allow for the Enter key to be pressed to validate coordinates
                //ProcessInput(_inputCoordinate);

                NotifyPropertyChanged(() => InputCoordinate);
            }
        }

        //TODO is this used anymore?
        public bool IsToolGenerated { get; set; }

        public virtual void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            // do nothing
        }

        public virtual string ProcessInput(string input)
        {
            return string.Empty;
        }

        public virtual void OnEditPropertiesDialogCommand(object obj)
        {
            var dlg = new ProEditPropertiesView();
            try
            {
                dlg.ShowDialog();
            }
            catch (Exception e)
            {
                if (e.Message.ToLower() == ProAppCoordConversionModule.Properties.Resources.CoordsOutOfBoundsMsg.ToLower())
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message + System.Environment.NewLine + ProAppCoordConversionModule.Properties.Resources.CoordsOutOfBoundsAddlMsg,
                        ProAppCoordConversionModule.Properties.Resources.CoordsoutOfBoundsCaption);
                }
                else
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message);
                }

            }

        }

        public virtual void OnCopyCommand(object obj)
        {
        }

        public virtual void OnPasteCommand(object obj)
        {
        }

        public virtual bool CheckMapLoaded()
        {
            return false;
        }
        
        public virtual void OnImportCSVFileCommand(object obj)
        {
            try
            {
                if (CheckMapLoaded())
                {
                    CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;

                    BrowseProjectFilter bf = new BrowseProjectFilter();
                    bf.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_textFiles_csv"));
                    bf.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_excel_files"));
                    
                    OpenItemDialog fileDialog = new OpenItemDialog
                    {
                        Title = Properties.Resources.FileDialogTitle,
                        MultiSelect = false,
                        BrowseFilter = bf,
                    };
                 
                    // attemp to import
                    var fieldVM = new SelectCoordinateFieldsViewModel();
                    var result = fileDialog.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {
                        var dlg = new ProSelectCoordinateFieldsView();

                        Item selectedItem = fileDialog.Items.First();
                        
                        var coordinates = new List<string>();
                        var extension = Path.GetExtension(selectedItem.Path);
                        switch (extension)
                        {
                            case ".csv":
                                ImportFromCSV(fieldVM, dlg, coordinates, selectedItem.Path);
                                break;
                            case ".xls":
                            case ".xlsx":
                                ImportFromExcel(dlg, selectedItem, fieldVM);
                                break;
                            default:
                                break;
                        }
                    }
                    CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = true;
                }
                else
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ProAppCoordConversionModule.Properties.Resources.AddLayerMsg,
                        ProAppCoordConversionModule.Properties.Resources.AddLayerCap);
                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Properties.Resources.DialogError);
                Debug.WriteLine(Properties.Resources.FailedImportCsv + ex.ToString());
            }
        }

        private void ImportFromCSV(SelectCoordinateFieldsViewModel fieldVM, ProSelectCoordinateFieldsView dlg, List<string> coordinates, string fileName)
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
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ProAppCoordConversionModule.Properties.Resources.MsgNoDataFound);
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
                                if(FieldCollection.Contains(field.Key))
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

                    CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
                    ProConvertTabViewModel pCvtTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;
                    ProCollectTabViewModel pCollectTabVM = pCvtTabVM.CollectTabView.DataContext as ViewModels.ProCollectTabViewModel;
                    pCollectTabVM.ImportCoordinates.Execute(ImportedData);
                }
            }
        }

        private void ImportFromExcel(ProSelectCoordinateFieldsView dlg, Item diag, SelectCoordinateFieldsViewModel fieldVM)
        {
            ImportedData = new List<Dictionary<string, Tuple<object, bool>>>();
            List<string> headers = new List<string>();
            FieldCollection = new List<object>();
            var filename = diag.Path;
            string selectedCol1Key = "", selectedCol2Key = "";
            var selectedColumn = fieldVM.SelectedFields.ToArray();
            var lstDictonary = new List<Dictionary<string, Tuple<object, bool>>>();
            var columnCollection = new List<string>();
            var fileExt = Path.GetExtension(filename); //get the extension of uploaded excel file
            var lstDictionary = ReadExcelInput(filename);

            var firstDict = lstDictionary.FirstOrDefault();
            headers = firstDict.Where(x => x.Key != "OBJECTID").Select(x => x.Key).ToList<string>();
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
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ProAppCoordConversionModule.Properties.Resources.MsgNoDataFound);
            }
            if (dlg.ShowDialog() == true)
            {
                foreach (var item in headers)
                {
                    columnCollection.Add(item);
                    if (item == fieldVM.SelectedFields.ToArray()[0])
                    {
                        SelectedField1 = item;
                        selectedCol1Key = item;
                    }
                    if (fieldVM.UseTwoFields)
                    {
                        if (item == fieldVM.SelectedFields.ToArray()[1])
                        {
                            SelectedField2 = item;
                            selectedCol2Key = item;
                        }
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
                    lstDictonary.Add(dict);
                }

                CoordinateConversionDockpaneViewModel ccVM = Module1.CoordinateConversionVM;
                ProConvertTabViewModel pCvtTabVM = ccVM.ConvertTabView.DataContext as ViewModels.ProConvertTabViewModel;
                ProCollectTabViewModel pCollectTabVM = pCvtTabVM.CollectTabView.DataContext as ViewModels.ProCollectTabViewModel;
                pCollectTabVM.ImportCoordinates.Execute(lstDictonary);
            }
        }

        public virtual List<Dictionary<string, Tuple<object, bool>>> ReadExcelInput(string fileName)
        {
            return new List<Dictionary<string, Tuple<object, bool>>>();
        }

        private void OnValidateMapPointInternal(object obj)
        {
            OnValidateMapPoint(obj);
        }

        public virtual void OnValidateMapPoint(object obj)
        {

        }

        private void OnSelectMapPointInternal(object obj)
        {
            OnMapPointSelection(obj);
        }

        public virtual bool OnNewMapPoint(object obj)
        {
            return true;
        }

        private void OnNewMapPointInternal(object obj)
        {
            OnNewMapPoint(obj);
        }

        public virtual void OnMapPointSelection(object obj)
        {

        }

        private void OnMouseMoveInternal(object obj)
        {
            OnMouseMove(obj);
        }

        public virtual bool OnMouseMove(object obj)
        {
            return true;
        }

    }

    public class ImportCoordinatesList
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class FieldsCollection
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
