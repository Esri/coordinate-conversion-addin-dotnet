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

using CoordinateToolLibrary.Helpers;
using CoordinateToolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.ViewModels
{
    public class EditOutputCoordinateViewModel : BaseViewModel
    {
        public EditOutputCoordinateViewModel() 
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.MainWindow.Title.Contains("ArcGIS Pro"))
            {
                CategoryList = new ObservableCollection<string>() { "DD", "DDM", "DMS" };
            }
            else
            {
                CategoryList = new ObservableCollection<string>() { "DD", "DDM", "DMS", "GARS", "MGRS", "USNG", "UTM" };
            }
            FormatList = new ObservableCollection<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Custom" };
            Sample = "Sample";
            Format = "Y-+0.####,X-+0.####";

            FormatExpanded = false;

            Mediator.Register(CoordinateToolLibrary.Constants.BroadcastCoordinateValues, OnHandleBCCValues);
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.RequestCoordinateBroadcast, null);

            ConfigCommand = new RelayCommand(OnConfigCommand);

            Mediator.Register(CoordinateToolLibrary.Constants.SpatialReferenceSelected, OnSpatialReferenceSelected);
        }

        public RelayCommand ConfigCommand { get; set; }
        private static Dictionary<CoordinateType, string> ctdict = new Dictionary<CoordinateType,string>();
        public ObservableCollection<string> CategoryList { get; set; }
        public ObservableCollection<string> FormatList { get; set; }
        public string WindowTitle { get; set; }
        public List<string> Names { get; set; }
        public string Sample { get; set; }

        private string _format = string.Empty;
        public string Format 
        {
            get
            {
                return _format;
            }

            set
            {
                _format = value;
                RaisePropertyChanged(() => Format);
                UpdateSample();
                if (DefaultFormats != null)
                {
                    var temp = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());
                    if (temp != null && temp.DefaultNameFormatDictionary != null && temp.DefaultNameFormatDictionary.Values != null)
                    {
                        if (!temp.DefaultNameFormatDictionary.Values.Contains(Format))
                        {
                            FormatSelection = "Custom";
                            FormatExpanded = true;
                        }
                    }
                }
            }
        }
        private string _categorySelection = string.Empty;
        public string CategorySelection 
        {
            get
            {
                return _categorySelection;
            }
            set
            {
                if(_categorySelection != value)
                {
                    _categorySelection = value;
                    OnCategorySelectionChanged();
                }
            }
        }

        public bool FormatExpanded { get; set; }

        private void OnCategorySelectionChanged()
        {
            if (string.IsNullOrWhiteSpace(CategorySelection))
                return;

            var list = GetFormatList(CategorySelection);

            if (list == null)
                return;

            list.Add("Custom");

            FormatList = list;

            if(!FormatList.Contains(FormatSelection) || FormatSelection == "Custom")
            {
                // update format selection
                FormatSelection = FormatList.FirstOrDefault();
            }

            RaisePropertyChanged(() => FormatList);

            OutputCoordItem.CType = GetCoordinateType();
            OutputCoordItem.Name = OutputCoordItem.CType.ToString();
        }
        private string _formatSelection;
        public string FormatSelection 
        {
            get
            {
                return _formatSelection;
            }
            set
            {
                if(_formatSelection != value)
                { 
                    _formatSelection = value;
                    OnFormatSelectionChanged();
                    RaisePropertyChanged(() => FormatSelection);
                }
            }
        }

        public ObservableCollection<DefaultFormatModel> DefaultFormats { get; set; }
        private OutputCoordinateModel _outputCoordItem = null;
        public OutputCoordinateModel OutputCoordItem
        {
            get
            {
                return _outputCoordItem;
            }
            set
            {
                _outputCoordItem = value;
                OnOutputCoordItemChanged();
                RaisePropertyChanged(() => OutputCoordItem);
            }
        }

        private void OnOutputCoordItemChanged()
        {
            if (OutputCoordItem == null)
                return;

            SelectCategory(OutputCoordItem.CType);

            SelectFormat(OutputCoordItem.Format);

            Format = OutputCoordItem.Format;
        }

        private void OnFormatSelectionChanged()
        {
            // if not custom, change format
            // and update sample

            if(FormatSelection != "Custom")
            {
                // get format from defaults

                Format = GetFormatFromDefaults();

                UpdateSample();
            }
            else
            {
                FormatExpanded = true;
                RaisePropertyChanged(() => FormatExpanded);
            }
        }

        private string GetFormatFromDefaults()
        {
            var item = DefaultFormats.First(i => i.CType == GetCoordinateType());

            if (item == null)
                return "No Format Found";

            return item.DefaultNameFormatDictionary[FormatSelection];
        }

        private void UpdateSample()
        {
            var type = GetCoordinateType();

            switch(type)
            {
                case CoordinateType.DD:
                    var dd = new CoordinateDD();

                    if (ctdict.ContainsKey(CoordinateType.DD))
                    {
                        CoordinateDD.TryParse(ctdict[type], out dd);
                    }

                    Sample = dd.ToString(Format, new CoordinateDDFormatter());

                    break;
                case CoordinateType.DDM:
                    var ddm = new CoordinateDDM();
                    
                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateDDM.TryParse(ctdict[type], out ddm);
                    }

                    Sample = ddm.ToString(Format, new CoordinateDDMFormatter());
                    break;
                case CoordinateType.DMS:
                    var dms = new CoordinateDMS();

                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateDMS.TryParse(ctdict[type], out dms);
                    }
                    Sample = dms.ToString(Format, new CoordinateDMSFormatter());
                    break;
                case CoordinateType.GARS:
                    var gars = new CoordinateGARS();

                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateGARS.TryParse(ctdict[type], out gars);
                    }

                    Sample = gars.ToString(Format, new CoordinateGARSFormatter());
                    break;
                case CoordinateType.MGRS:
                    var mgrs = new CoordinateMGRS();

                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateMGRS.TryParse(ctdict[type], out mgrs);
                    }

                    Sample = mgrs.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.USNG:
                    var usng = new CoordinateUSNG();

                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateUSNG.TryParse(ctdict[type], out usng);
                    }

                    Sample = usng.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.UTM:
                    var utm = new CoordinateUTM();

                    if(ctdict.ContainsKey(type))
                    {
                        CoordinateUTM.TryParse(ctdict[type], out utm);
                    }

                    Sample = utm.ToString(Format, new CoordinateUTMFormatter());
                    break;
                default:
                    break;
            }

            RaisePropertyChanged(() => Sample);
        }

        private void OnHandleBCCValues(object obj)
        {
            var dict = obj as Dictionary<CoordinateType, string>;

            if (dict != null)
            {
                ctdict.Clear();
                foreach (var item in dict)
                    ctdict.Add(item.Key, item.Value);
            }
        }



        private CoordinateType GetCoordinateType()
        {
            CoordinateType type;

            if (Enum.TryParse<CoordinateType>(CategorySelection, out type))
                return type;

            return CoordinateType.Unknown;
        }

        private ObservableCollection<string> GetFormatList(string CategorySelection)
        {
            var item = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());

            if (item == null)
                return null;

            return new ObservableCollection<string>(item.DefaultNameFormatDictionary.Keys);
        }

        private void SelectFormat(string format)
        {
            var defaultFormat = GetFormatSample(format);

            foreach( var item in FormatList)
            {
                if (item == defaultFormat)
                {
                    FormatSelection = item;
                    return;
                }
            }

            FormatSelection = FormatList.FirstOrDefault();
        }

        private string GetFormatSample(string format)
        {
            if (OutputCoordItem == null)
                return string.Empty;

            var def = DefaultFormats.FirstOrDefault(i => i.CType == OutputCoordItem.CType);

            if (def == null)
                return string.Empty;

            foreach(var item in def.DefaultNameFormatDictionary)
            {
                if(item.Value == format)
                {
                    return item.Key;
                }
            }

            return "Custom";
        }

        private void SelectCategory(CoordinateType coordinateType)
        {
            foreach(var item in CategoryList)
            {
                if(item == coordinateType.ToString())
                {
                    CategorySelection = item;
                    RaisePropertyChanged(() => CategorySelection);
                }
            }
        }

        private void OnConfigCommand(object obj)
        {
            // need to get consumer to ask for spatial reference
            Mediator.NotifyColleagues(CoordinateToolLibrary.Constants.SelectSpatialReference, null);
        }

        private void OnSpatialReferenceSelected(object obj)
        {
            var data = obj as string;

            if (string.IsNullOrWhiteSpace(data))
                return;

            var temp = data.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            if(temp.Count() == 2)
            {
                OutputCoordItem.SRFactoryCode = Convert.ToInt32(temp[0]);
                OutputCoordItem.SRName = temp[1];
                RaisePropertyChanged(() => OutputCoordItem);
            }
        }
    }
}
