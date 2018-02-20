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
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CoordinateConversionLibrary.ViewModels
{
    public class TabBaseViewModel : BaseViewModel
    {
        public TabBaseViewModel()
        {
            HasInputError = false;
            IsHistoryUpdate = true;
            IsToolGenerated = false;
            ToolMode = MapPointToolMode.Unknown;

            // commands
            EditPropertiesDialogCommand = new RelayCommand(OnEditPropertiesDialogCommand);
            ImportCSVFileCommand = new RelayCommand(OnImportCSVFileCommand);

            Mediator.Register(CoordinateConversionLibrary.Constants.NEW_MAP_POINT, OnNewMapPointInternal);
            Mediator.Register(CoordinateConversionLibrary.Constants.MOUSE_MOVE_POINT, OnMouseMoveInternal);
            Mediator.Register(CoordinateConversionLibrary.Constants.TAB_ITEM_SELECTED, OnTabItemSelected);
            Mediator.Register(CoordinateConversionLibrary.Constants.SetToolMode, (mode) =>
            {
                MapPointToolMode eMode = MapPointToolMode.Unknown;
                Enum.TryParse<MapPointToolMode>(mode.ToString(), out eMode);
                ToolMode = eMode;
            });

            configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
                .RegisterHandler(n => n.DisplayCoordinateType, OnDisplayCoordinateTypeChanged);
        }

        PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        public RelayCommand EditPropertiesDialogCommand { get; set; }
        public RelayCommand ImportCSVFileCommand { get; set; }

        //TODO is this used anymore?
        public bool IsHistoryUpdate { get; set; }

        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                RaisePropertyChanged(() => HasInputError);
            }
        }

        public MapPointToolMode ToolMode { get; set; }

        private bool isActiveTab = true;
        /// <summary>
        /// Property to keep track of which tab/viewmodel is the active item
        /// </summary>
        public bool IsActiveTab
        {
            get
            {
                return isActiveTab;
            }
            set
            {
                //TODO do we need a reset?
                //Reset(true);
                isActiveTab = value;
                RaisePropertyChanged(() => IsActiveTab);
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
                //Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

                RaisePropertyChanged(() => InputCoordinate);
            }
        }

        //TODO is this used anymore?
        public bool IsToolGenerated { get; set; }

        public virtual void OnDisplayCoordinateTypeChanged(CoordinateConversionLibraryConfig obj)
        {
            // do nothing
        }

        public virtual void ProcessInput(string input)
        {
            return;
        }

        public void OnEditPropertiesDialogCommand(object obj)
        {
            var dlg = new EditPropertiesView();
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

        private void OnImportCSVFileCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false; 

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.Filter = "csv files|*.csv";

            var result = fileDialog.ShowDialog();
            if(result.HasValue && result.Value == true)
            {
                // attemp to import
                var fieldVM = new SelectCoordinateFieldsViewModel();

                var headers = ImportCSV.GetHeaders(File.OpenRead(fileDialog.FileName));
                foreach (var header in headers)
                {
                    fieldVM.AvailableFields.Add(header);
                    System.Diagnostics.Debug.WriteLine("header : {0}", header);
                }

                var dlg = new SelectCoordinateFieldsView();
                dlg.DataContext = fieldVM;
                if (dlg.ShowDialog() == true)
                {
                    var lists = ImportCSV.Import<ImportCoordinatesList>(File.OpenRead(fileDialog.FileName), fieldVM.SelectedFields.ToArray());

                    var coordinates = new List<string>();

                    foreach(var item in lists)
                    {
                        var sb = new StringBuilder();
                        sb.Append(item.lat.Trim());
                        if (fieldVM.UseTwoFields)
                            sb.Append(string.Format(" {0}", item.lon.Trim()));

                        coordinates.Add(sb.ToString());
                    }

                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.IMPORT_COORDINATES, coordinates);
                }
            }

            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = true; 
        }

        private void OnNewMapPointInternal(object obj)
        {
            OnNewMapPoint(obj);
        }

        public virtual bool OnNewMapPoint(object obj)
        {
            return IsActiveTab;
        }

        private void OnMouseMoveInternal(object obj)
        {
            OnMouseMove(obj);
        }

        public virtual bool OnMouseMove(object obj)
        {
            return IsActiveTab;
        }

        /// <summary>
        /// Handler for the tab item selected event
        /// Helps keep track of which tab item/viewmodel is active
        /// </summary>
        /// <param name="obj">bool if selected or not</param>
        private void OnTabItemSelected(object obj)
        {
            if (obj == null)
                return;

            IsActiveTab = (obj == this);
        }
    }

    public class ImportCoordinatesList
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }
}
