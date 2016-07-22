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

using CoordinateConversionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class ConvertTabViewModel : TabBaseViewModel
    {
        public ConvertTabViewModel()
        {
            InputCoordinateHistoryList = new ObservableCollection<string>();

            CopyAllCommand = new RelayCommand(OnCopyAllCommand);
        }

        public ObservableCollection<string> InputCoordinateHistoryList { get; set; }

        public RelayCommand CopyAllCommand { get; set; }

        internal void OnCopyAllCommand(object obj)
        {
            Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.CopyAllCoordinateOutputs, InputCoordinate);
        }

    }
}
