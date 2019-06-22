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
using CoordinateConversionLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CoordinateConversionLibrary.ViewModels
{
    public class EditPropertiesViewModel : BaseViewModel
    {
        public EditPropertiesViewModel()
        {
            IsInitialCall = true;
            DisplayAmbiguousCoordsDlg = CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg;
            OKButtonPressedCommand = new RelayCommand(OnOkButtonPressedCommand);
            CancelButtonPressedCommand = new RelayCommand(OnCancelButtonPressedCommand);
            FormatList = new ObservableCollection<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Custom" };
            Sample = "Sample";
            var removedType = new string[] { "Custom", "None" };
            IsEnableExpander = false;
            var coordinateCollections = Enum.GetValues(typeof(CoordinateTypes)).Cast<CoordinateTypes>().Where(x => !removedType.Contains(x.ToString()));
            CoordinateTypeCollections = new ObservableCollection<CoordinateTypes>(coordinateCollections);

            DefaultFormats = CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList;
            SelectedCoordinateType = CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType;
            FormatSelection = CoordinateConversionLibraryConfig.AddInConfig.FormatSelection;
            if (FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                _categorySelection = FormatList.Where(x => x == CoordinateConversionLibrary.Properties.Resources.CustomString).FirstOrDefault();
                FormatExpanded = true;
                IsEnableExpander = true;
                Format = CoordinateBase.InputCustomFormat;
                IsHemisphereIndicatorChecked = CoordinateConversionLibraryConfig.AddInConfig.IsHemisphereIndicatorChecked;
                IsPlusHyphenChecked = CoordinateConversionLibraryConfig.AddInConfig.IsPlusHyphenChecked;
            }
            else
            {
                _categorySelection = FormatList.FirstOrDefault();
                FormatExpanded = false;
                IsEnableExpander = false;
                CoordinateConversionLibraryConfig.AddInConfig.IsHemisphereIndicatorChecked = true;
            }
            ShowSymbols();
            IsInitialCall = false;
        }

        public bool IsInitialCall { get; set; }
        public RelayCommand OKButtonPressedCommand { get; set; }
        public RelayCommand CancelButtonPressedCommand { get; set; }
        private ObservableCollection<CoordinateTypes> _coordinateTypeCollections;
        public ObservableCollection<CoordinateTypes> CoordinateTypeCollections
        {
            get
            {
                return _coordinateTypeCollections;
            }
            set
            {
                _coordinateTypeCollections = value;
                RaisePropertyChanged(() => CoordinateTypeCollections);
            }
        }

        private bool showPlusForDirection;
        public bool ShowPlusForDirection
        {
            get { return showPlusForDirection; }
            set
            {
                showPlusForDirection = value;
                CoordinateBase.ShowPlus = value;
                UpdateCustomFormatPreview();
                RaisePropertyChanged(() => ShowPlusForDirection);
            }
        }

        private bool showHyphenForDirection;
        public bool ShowHyphenForDirection
        {
            get { return showHyphenForDirection; }
            set
            {
                showHyphenForDirection = value;
                CoordinateBase.ShowHyphen = value;
                UpdateCustomFormatPreview();
                RaisePropertyChanged(() => ShowHyphenForDirection);
            }
        }

        private bool isHemisphereIndicatorChecked;
        public bool IsHemisphereIndicatorChecked
        {
            get { return isHemisphereIndicatorChecked; }
            set
            {
                isHemisphereIndicatorChecked = value;
                CoordinateBase.ShowHemisphere = value;
                ShowPlusForDirection = false;
                ShowHyphenForDirection = false;
                RaisePropertyChanged(() => IsHemisphereIndicatorChecked);
            }
        }

        private CoordinateTypes _selectedCoordinateType { get; set; }
        public CoordinateTypes SelectedCoordinateType
        {
            get
            {
                return _selectedCoordinateType;
            }
            set
            {
                _selectedCoordinateType = value;
                CoordinateBase.InputCategorySelection = value;
                if (!IsInitialCall)
                    CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
                OnCategorySelectionChanged();
                RaisePropertyChanged(() => SelectedCoordinateType);
            }
        }

        private Visibility plusHyphenForDirectionVisibility;
        public Visibility PlusHyphenForDirectionVisibility
        {
            get { return plusHyphenForDirectionVisibility; }
            set
            {
                plusHyphenForDirectionVisibility = value;
                RaisePropertyChanged(() => PlusHyphenForDirectionVisibility);
            }
        }

        private Visibility hemisphereIndicatorVisibility;
        public Visibility HemisphereIndicatorVisibility
        {
            get { return hemisphereIndicatorVisibility; }
            set
            {
                hemisphereIndicatorVisibility = value;
                RaisePropertyChanged(() => HemisphereIndicatorVisibility);
            }
        }

        private bool isPlusHyphenChecked;
        public bool IsPlusHyphenChecked
        {
            get { return isPlusHyphenChecked; }
            set
            {
                isPlusHyphenChecked = value;
                IsPlusHyphenEnabled = value;
                RaisePropertyChanged(() => IsPlusHyphenChecked);
            }
        }

        private bool isHemisphereIndicatorEnabled;
        public bool IsHemisphereIndicatorEnabled
        {
            get { return isHemisphereIndicatorEnabled; }
            set
            {
                isHemisphereIndicatorEnabled = value;
                RaisePropertyChanged(() => IsHemisphereIndicatorEnabled);
            }
        }

        private bool isPlusHyphenEnabled;
        public bool IsPlusHyphenEnabled
        {
            get { return isPlusHyphenEnabled; }
            set
            {
                isPlusHyphenEnabled = value;
                RaisePropertyChanged(() => IsPlusHyphenEnabled);
            }
        }

        private string customFormatPreview;
        public string CustomFormatPreview
        {
            get { return customFormatPreview; }
            set
            {
                customFormatPreview = value;
                RaisePropertyChanged(() => CustomFormatPreview);
            }
        }

        private string _format = string.Empty;
        public string Format
        {
            get
            {
                return _format;
            }

            set
            {
                if (IsNotValidInput(value))
                {
                    IsValidFormat = false;
                    throw new ArgumentException(CoordinateConversionLibrary.Properties.Resources.SpecialCharactersValidationMsg);
                }
                else
                {
                    _format = value;
                    IsValidFormat = true;
                    UpdateCustomFormatPreview();
                    RaisePropertyChanged(() => Format);
                }

            }
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
                if (_formatSelection != value)
                {
                    _formatSelection = value;
                    CoordinateBase.InputFormatSelection = value;
                    OnFormatSelectionChanged();
                    RaisePropertyChanged(() => FormatSelection);
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
                if (_categorySelection != value)
                {
                    _categorySelection = value;
                    OnCategorySelectionChanged();
                }
            }
        }

        public ObservableCollection<string> CategoryList { get; set; }
        public ObservableCollection<string> FormatList { get; set; }
        public bool IsValidFormat { get; set; }
        private static Dictionary<CoordinateType, string> ctdict = new Dictionary<CoordinateType, string>();
        public string Sample { get; set; }
        private bool _formatExpanded { get; set; }
        public bool FormatExpanded
        {
            get
            {
                return _formatExpanded;
            }
            set
            {
                _formatExpanded = value;
                RaisePropertyChanged(() => FormatExpanded);
            }
        }
        private bool _isEnableExpander { get; set; }
        public bool IsEnableExpander
        {
            get
            {
                return _isEnableExpander;
            }
            set
            {
                _isEnableExpander = value;
                RaisePropertyChanged(() => IsEnableExpander);
            }
        }
        public bool DisplayAmbiguousCoordsDlg { get; set; }
        public ObservableCollection<DefaultFormatModel> DefaultFormats { get; set; }

        private bool? dialogResult = null;
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                RaisePropertyChanged(() => DialogResult);
            }
        }

        /// <summary>
        /// Handler for when someone closes the dialog with the OK button
        /// </summary>
        /// <param name="obj"></param>
        private void OnOkButtonPressedCommand(object obj)
        {
            if (!IsValidFormat)
                return;

            CoordinateConversionLibraryConfig.AddInConfig.DisplayCoordinateType = SelectedCoordinateType;
            CoordinateConversionLibraryConfig.AddInConfig.ShowPlusForDirection = ShowPlusForDirection;
            CoordinateConversionLibraryConfig.AddInConfig.ShowHyphenForDirection = ShowHyphenForDirection;
            CoordinateConversionLibraryConfig.AddInConfig.IsHemisphereIndicatorChecked = IsHemisphereIndicatorChecked;
            CoordinateConversionLibraryConfig.AddInConfig.IsPlusHyphenChecked = IsPlusHyphenChecked;
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = DisplayAmbiguousCoordsDlg;
            CoordinateConversionLibraryConfig.AddInConfig.FormatSelection = FormatSelection;
            if (FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                CoordinateConversionLibraryConfig.AddInConfig.CategorySelection = CategorySelection;
            }
            else
            {
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
                CoordinateConversionLibraryConfig.AddInConfig.CategorySelection = CategorySelection;
            }
            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();

            CoordinateBase.InputCustomFormat = Format;
            // close dialog
            DialogResult = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnCancelButtonPressedCommand(object obj)
        {
            DialogResult = true;
        }

        private void OnFormatSelectionChanged()
        {
            if (FormatSelection != Properties.Resources.CustomString)
            {
                _format = GetFormatFromDefaults();
                UpdateSample();
                IsEnableExpander = false;
                FormatExpanded = false;
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = false;
            }
            else
            {
                IsEnableExpander = true;
                FormatExpanded = true;
                CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat = true;
                IsHemisphereIndicatorChecked = false;
                IsPlusHyphenChecked = false;
                RaisePropertyChanged(() => FormatExpanded);
            }
            ShowSymbols();
        }
        private string GetFormatFromDefaults()
        {
            var item = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());
            if (item == null)
                return Properties.Resources.StringNoFormatFound;
            return item.DefaultNameFormatDictionary.Select(x => x.Value).FirstOrDefault();
        }
        private CoordinateType GetCoordinateType()
        {
            CoordinateType type;

            var selectedCoordinateType = Convert.ToString(SelectedCoordinateType);
            if (Enum.TryParse<CoordinateType>(selectedCoordinateType, out type))
                return type;

            return CoordinateType.Unknown;
        }
        private void UpdateSample()
        {
            var type = GetCoordinateType();

            switch (type)
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

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateDDM.TryParse(ctdict[type], out ddm);
                    }

                    Sample = ddm.ToString(Format, new CoordinateDDMFormatter());
                    break;
                case CoordinateType.DMS:
                    var dms = new CoordinateDMS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateDMS.TryParse(ctdict[type], out dms);
                    }
                    Sample = dms.ToString(Format, new CoordinateDMSFormatter());
                    break;
                case CoordinateType.GARS:
                    var gars = new CoordinateGARS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateGARS.TryParse(ctdict[type], out gars);
                    }

                    Sample = gars.ToString(Format, new CoordinateGARSFormatter());
                    break;
                case CoordinateType.MGRS:
                    var mgrs = new CoordinateMGRS();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateMGRS.TryParse(ctdict[type], out mgrs);
                    }

                    Sample = mgrs.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.USNG:
                    var usng = new CoordinateUSNG();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateUSNG.TryParse(ctdict[type], out usng);
                    }

                    Sample = usng.ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.UTM:
                    var utm = new CoordinateUTM();

                    if (ctdict.ContainsKey(type))
                    {
                        CoordinateUTM.TryParse(ctdict[type], out utm);
                    }

                    Sample = utm.ToString(Format, new CoordinateUTMFormatter());
                    break;
                default:
                    Sample = FormatList.FirstOrDefault();
                    break;
            }

            RaisePropertyChanged(() => Sample);
        }

        private void OnCategorySelectionChanged()
        {
            var selectedCoordinateType = Convert.ToString(SelectedCoordinateType);
            if (string.IsNullOrWhiteSpace(selectedCoordinateType))
                return;

            var list = GetFormatList(selectedCoordinateType);

            if (list == null)
                return;
            if (selectedCoordinateType != CoordinateType.Default.ToString())
                list.Add(Properties.Resources.CustomString);

            FormatList = list;
            if ((
                (!FormatList.Contains(FormatSelection) || FormatSelection == Properties.Resources.CustomString)
                && !CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
                )
            {
                // update format selection
                FormatSelection = FormatList.FirstOrDefault();
            }

            RaisePropertyChanged(() => FormatList);

            if (!CoordinateConversionLibraryConfig.AddInConfig.IsCustomFormat)
                Format = GetFormatFromDefaults();
            else
                Format = CoordinateBase.InputCustomFormat;
            RaisePropertyChanged(() => Format);
        }

        private ObservableCollection<string> GetFormatList(string CategorySelection)
        {
            var item = DefaultFormats.FirstOrDefault(i => i.CType == GetCoordinateType());

            if (item == null)
                return null;

            return new ObservableCollection<string>(item.DefaultNameFormatDictionary.Keys);
        }

        private void SelectCategory(CoordinateType coordinateType)
        {
            foreach (var item in CategoryList)
            {
                if (item == coordinateType.ToString())
                {
                    CategorySelection = item;
                    RaisePropertyChanged(() => SelectedCoordinateType);
                }
            }
        }

        private void SelectFormat(string format)
        {
            var defaultFormat = GetFormatSample(format);

            foreach (var item in FormatList)
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
            var cType = GetCoordinateType();
            var def = DefaultFormats.FirstOrDefault(i => i.CType == cType);

            if (def == null)
                return string.Empty;

            foreach (var item in def.DefaultNameFormatDictionary)
            {
                if (item.Value == format)
                {
                    return item.Key;
                }
            }

            return Properties.Resources.CustomString;
        }

        private bool IsNotValidInput(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var type = GetCoordinateType();
            if (type == CoordinateType.MGRS || type == CoordinateType.USNG || type == CoordinateType.UTM)
                return false;

            if (type == CoordinateType.DMS || type == CoordinateType.DDM)
            {
                var result = ValidationForDDM_DMS(value);
                if (result)
                    return result;
                return (value.Contains("N") || value.Contains("S")
                || value.Contains("E") || value.Contains("W")
                || value.Contains("n") || value.Contains("s")
                || value.Contains("e") || value.Contains("w"));
            }
            else
            {
                return (value.Contains("+") || value.Contains("-")
                || value.Contains("N") || value.Contains("S")
                || value.Contains("E") || value.Contains("W")
                || value.Contains("n") || value.Contains("s")
                || value.Contains("e") || value.Contains("w"));
            }
        }

        private static bool ValidationForDDM_DMS(string value)
        {
            int latIndex = value.IndexOf('A'), lonIndex = value.IndexOf('X');
            int startIndex = -1, nextIndex = -1;
            var isLatFirst = latIndex < lonIndex;

            if (new[] { "+", "-" }.Any(value.Substring(0, isLatFirst ? latIndex : lonIndex).Contains))
                return true;

            //get next prefix start index
            if (value.Contains(isLatFirst ? "C" : "Z"))
                startIndex = value.IndexOf(isLatFirst ? "C" : "Z");
            else if (value.Contains(isLatFirst ? "B" : "Y"))
                startIndex = value.IndexOf(isLatFirst ? "B" : "Y");
            else if (value.Contains(isLatFirst ? "A" : "X"))
                startIndex = value.IndexOf(isLatFirst ? "A" : "X");

            //get next prefix
            nextIndex = value.IndexOf(isLatFirst ? "X" : "A");
            var nextPrefix = value.Substring(startIndex, (nextIndex - startIndex));

            if (new[] { "+", "-" }.Any(nextPrefix.Contains))
                return true;
            return false;
        }

        private void ShowSymbols()
        {
            if ((SelectedCoordinateType == CoordinateTypes.DD || SelectedCoordinateType == CoordinateTypes.DDM
                || SelectedCoordinateType == CoordinateTypes.DMS || SelectedCoordinateType == CoordinateTypes.Default)
                && FormatSelection == CoordinateConversionLibrary.Properties.Resources.CustomString)
            {
                PlusHyphenForDirectionVisibility = Visibility.Visible;
                HemisphereIndicatorVisibility = Visibility.Visible;
                if (IsInitialCall)
                {
                    IsHemisphereIndicatorChecked = CoordinateConversionLibraryConfig.AddInConfig.IsHemisphereIndicatorChecked;
                    ShowHyphenForDirection = CoordinateConversionLibraryConfig.AddInConfig.ShowHyphenForDirection;
                    ShowPlusForDirection = CoordinateConversionLibraryConfig.AddInConfig.ShowPlusForDirection;
                    IsPlusHyphenChecked = CoordinateConversionLibraryConfig.AddInConfig.IsPlusHyphenChecked;
                }
            }
            else
            {
                PlusHyphenForDirectionVisibility = Visibility.Collapsed;
                HemisphereIndicatorVisibility = Visibility.Collapsed;
                ShowPlusForDirection = false;
                ShowHyphenForDirection = false;
                IsHemisphereIndicatorChecked = false;
            }
        }

        private void UpdateCustomFormatPreview()
        {
            if (!string.IsNullOrEmpty(Format) && Format.Contains("X"))
            {
                var coord = Format.Split('X');
                coord[0] = coord[0].TrimEnd(' ');
                coord[1] = "X" + coord[1];
                if (ShowPlusForDirection && !ShowHyphenForDirection)
                {
                    coord[0] = "+" + coord[0];
                    coord[1] = "+" + coord[1];
                }
                else
                {
                    coord[0].Replace("+", "");
                    coord[1].Replace("+", "");
                }
                if (ShowHyphenForDirection && !ShowPlusForDirection)
                {
                    coord[0] = "-" + coord[0];
                    coord[1] = "-" + coord[1];
                }
                else
                {
                    coord[0].Replace("-", "");
                    coord[1].Replace("-", "");
                }
                if (ShowHyphenForDirection && ShowPlusForDirection)
                {
                    coord[0] = "+/-" + coord[0];
                    coord[1] = "+/-" + coord[1];
                }
                else
                {
                    coord[0].Replace("+/-", "");
                    coord[1].Replace("+/-", "");
                }
                CustomFormatPreview = coord[0] + " " + coord[1];
                if (IsHemisphereIndicatorChecked)
                {
                    ParseCustomFormats(Format);
                }
                else
                {
                    coord[0].Replace("N", "");
                    coord[1].Replace("E", "");
                    CustomFormatPreview = coord[0] + " " + coord[1];
                }
            }
        }

        private void ParseCustomFormats(string format)
        {
            bool startIndexNeeded = true, endIndexNeeded = true, skipLonChar = false, skipLatChar = false;
            int latIndex = -1, lonIndex = -1;
            var formatChar = format.ToArray();
            var endIndexCollection = new List<int>();
            for (int i = 0; i < formatChar.Length; i++)
            {
                var c = formatChar[i];
                if ((c == 'X' && SelectedCoordinateType == CoordinateTypes.DD)
                    || ((c == 'X' || c == 'Y') && SelectedCoordinateType == CoordinateTypes.DDM)
                    || ((c == 'X' || c == 'Y' || c == 'Z') && SelectedCoordinateType == CoordinateTypes.DMS))
                {
                    startIndexNeeded = true;
                    lonIndex = i;
                }
                else if ((c == 'Y' && SelectedCoordinateType == CoordinateTypes.DD)
                    || ((c == 'A' || c == 'B') && SelectedCoordinateType == CoordinateTypes.DDM)
                    || ((c == 'A' || c == 'B' || c == 'C') && SelectedCoordinateType == CoordinateTypes.DMS))
                {
                    startIndexNeeded = true;
                    latIndex = i;
                }
                else if (startIndexNeeded && (c == '#' || c == '.' || c == '0'))
                {
                    startIndexNeeded = false;
                    endIndexNeeded = true;
                }
                else if (endIndexNeeded && (c != '#' && c != '.' && c != '0'))
                {
                    endIndexCollection.Add(i);
                    endIndexNeeded = false;
                }
            }
            if (endIndexCollection.Count == 1)
                endIndexCollection.Add(format.Length);
            if (lonIndex != -1 && latIndex != -1)
            {
                var lonVal = endIndexCollection.Where(x => x > latIndex).Any() ?
                    endIndexCollection.Max() : endIndexCollection.Where(x => x > lonIndex).Min();
                var latVal = endIndexCollection.Where(x => x > latIndex).Any() ?
                    endIndexCollection.Where(x => x > latIndex).Min() : endIndexCollection.Max();
                if (SelectedCoordinateType == CoordinateTypes.DMS || SelectedCoordinateType == CoordinateTypes.DDM)
                {
                    var nextLatChar = format.ElementAt(latVal);
                    var nextLonChar = format.ElementAt(lonVal);
                    if (SelectedCoordinateType == CoordinateTypes.DMS)
                    {
                        skipLatChar = ((nextLatChar == '"' & format.Contains('C'))
                            || (nextLatChar == '\'' & format.Contains('B'))
                            || (nextLatChar == '°' & format.Contains('A')));
                        skipLonChar = ((nextLonChar == '"' & format.Contains('Z'))
                            || (nextLonChar == '\'' & format.Contains('Y'))
                            || (nextLonChar == '°' & format.Contains('X')));
                    }
                    else if (SelectedCoordinateType == CoordinateTypes.DDM)
                    {
                        skipLatChar = ((nextLatChar == '\'' & format.Contains('B'))
                            || (nextLatChar == '°' & format.Contains('A')));
                        skipLonChar = ((nextLonChar == '\'' & format.Contains('Y'))
                            || (nextLonChar == '°' & format.Contains('X')));
                    }
                    lonVal = skipLonChar ? lonVal + 1 : lonVal;
                    latVal = skipLatChar ? latVal + 1 : latVal;
                }
                format = format.Substring(0, lonVal) + "E" + format.Substring(lonVal);
                format = format.Substring(0, latVal) + "N" + format.Substring(latVal);
            }
            CustomFormatPreview = format;
        }
    }
}