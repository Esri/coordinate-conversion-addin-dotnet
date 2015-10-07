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
        }

        public ObservableCollection<string> CategoryList { get; set; }
        public ObservableCollection<string> FormatList { get; set; }
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
        public string CategorySelection { get; set; }
        public string FormatSelection { get; set; }

        //public RelayCommand UpdateSampleCommand{get;set;}

        //private void OnUpdateSampleCommand(object obj)
        //{
        //    UpdateSample();
        //}

        public void UpdateFormat()
        {

        }

        public void UpdateSample()
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
    }
}
