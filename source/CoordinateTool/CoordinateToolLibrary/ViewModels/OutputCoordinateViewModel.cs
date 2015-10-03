using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Helpers;

namespace CoordinateToolLibrary.ViewModels
{
    public class OutputCoordinateViewModel : BaseViewModel
    {
        public OutputCoordinateViewModel() 
        {
            ConfigCommand = new RelayCommand(OnConfigCommand);
            ExpandCommand = new RelayCommand(OnExpandCommand);
            DeleteCommand = new RelayCommand(OnDeleteCommand);
            CopyCommand = new RelayCommand(OnCopyCommand);

            //init a few sample items
            OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            var tempProps = new Dictionary<string, string>() { { "Lat", "70.49N" }, { "Lon", "40.32W" } };
            var mgrsProps = new Dictionary<string, string>() { { "GZone", "17T" }, { "GSquare", "NE" }, { "Northing", "86309" }, { "Easting", "77770" } };
            OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DD", CType = CoordinateType.DD, OutputCoordinate = "70.49N 40.32W", Props = tempProps });
            OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DMS", CType = CoordinateType.DMS, OutputCoordinate = "40°26'46\"N,79°58'56\"W", Props = tempProps, Format = "A##°B##'C##\"N X###°Y##'Z##\"E" });
            OutputCoordinateList.Add(new OutputCoordinateModel { Name = "MGRS", CType = CoordinateType.MGRS, OutputCoordinate = @"", Props = mgrsProps, Format = "Z S E# N#" });
            OutputCoordinateList.Add(new OutputCoordinateModel { Name = "UTM", CType = CoordinateType.UTM, OutputCoordinate = @"", Format = "Z#H E# N#" });
        }

        /// <summary>
        /// The bound list.
        /// </summary>
        public ObservableCollection<OutputCoordinateModel> OutputCoordinateList { get; set; }

        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ConfigCommand { get; set; }
        public RelayCommand ExpandCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }

        //public void UpdateCoordinates()

        // copy parameter to clipboard
        private void OnCopyCommand(object obj)
        {
            var coord = obj as string;

            if(!string.IsNullOrWhiteSpace(coord))
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(coord);
            }
        }

        private void OnDeleteCommand(object obj)
        {
            var name = obj as string;

            if (!string.IsNullOrEmpty(name))
            {
                foreach (var item in OutputCoordinateList)
                {
                    if (item.Name == name)
                    {
                        OutputCoordinateList.Remove(item);
                        return;
                    }
                }
            }
        }

        private void OnExpandCommand(object obj)
        {
            var name = obj as string;

            if(!string.IsNullOrWhiteSpace(name))
            {
                foreach(var item in OutputCoordinateList)
                {
                    if(item.Name == name)
                    {
                        item.ToggleVisibility();
                        return;
                    }
                }
            }
        }

        private void OnConfigCommand(object obj)
        {
            //System.Windows.MessageBox.Show(string.Format("Configure {0}.", obj as string));
        }


    }
}
