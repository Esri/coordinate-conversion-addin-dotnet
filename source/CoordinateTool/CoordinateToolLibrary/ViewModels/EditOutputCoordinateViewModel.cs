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
            CategoryList = new ObservableCollection<string>() { "DD", "DDM", "DMS", "GARS", "MGRS", "USNG", "UTM"};
            FormatList = new ObservableCollection<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Custom" };
            Sample = "Sample";
            Format = "Y-+0.####,X-+0.####";

            //UpdateSampleCommand = new RelayCommand(OnUpdateSampleCommand);
            FormatExpanded = false;
        }

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

            RaisePropertyChanged(() => FormatList);
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
                    Sample = new CoordinateDD(40.1234, -70.5678).ToString(Format, new CoordinateDDFormatter());
                    break;
                case CoordinateType.DDM:
                    Sample = new CoordinateDDM(40, 20.1234, -70, 14.5678).ToString(Format, new CoordinateDDMFormatter());
                    break;
                case CoordinateType.DMS:
                    Sample = new CoordinateDMS(40, 20, 17.1234, -70, 30, 18.5678).ToString(Format, new CoordinateDMSFormatter());
                    break;
                case CoordinateType.GARS:
                    Sample = new CoordinateGARS(201, "LW", 1, 2).ToString(Format, new CoordinateGARSFormatter());
                    break;
                case CoordinateType.MGRS:
                    Sample = new CoordinateMGRS("24W", "VC", 61830, 66186).ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.USNG:
                    Sample = new CoordinateUSNG("24W", "VC", 61830, 66186).ToString(Format, new CoordinateMGRSFormatter());
                    break;
                case CoordinateType.UTM:
                    Sample = new CoordinateUTM(24, "N", 461830, 776618).ToString(Format, new CoordinateUTMFormatter());
                    break;
                default:
                    break;
            }

            RaisePropertyChanged(() => Sample);
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
                    
                    RaisePropertyChanged(() => FormatSelection);
                    return;
                }
            }
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
    }
}
