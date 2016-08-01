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
using System.Linq;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;

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

        private bool isActiveTab = false;
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
                var tempDD = ProcessInput(_inputCoordinate);

                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.RequestOutputUpdate, null);

                RaisePropertyChanged(() => InputCoordinate);
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
            return input;
        }

        public void OnEditPropertiesDialogCommand(object obj)
        {
            var dlg = new EditPropertiesView();

            dlg.ShowDialog();
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
}
