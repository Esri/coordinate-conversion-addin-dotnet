using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Helpers;
using CoordinateToolLibrary.Views;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

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
            //OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            ////var tempProps = new Dictionary<string, string>() { { "Lat", "70.49N" }, { "Lon", "40.32W" } };
            ////var mgrsProps = new Dictionary<string, string>() { { "GZone", "17T" }, { "GSquare", "NE" }, { "Northing", "86309" }, { "Easting", "77770" } };
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DD", CType = CoordinateType.DD, OutputCoordinate = "70.49N 40.32W" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DMS", CType = CoordinateType.DMS, OutputCoordinate = "40°26'46\"N,79°58'56\"W", Format = "A#°B0'C0\"N X#°Y0'Z0\"E" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "MGRS", CType = CoordinateType.MGRS, OutputCoordinate = @"", Format = "Z S E# N#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "UTM", CType = CoordinateType.UTM, OutputCoordinate = @"", Format = "Z#H E# N#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "GARS", CType = CoordinateType.GARS, OutputCoordinate = @"", Format = "X#YQK" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "USNG", CType = CoordinateType.USNG, OutputCoordinate = @"", Format = "Z S E# N#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DDM", CType = CoordinateType.DDM, OutputCoordinate = @"", Format = "A# B#.#### N X# Y#.#### E" });

            //DefaultFormatList = new ObservableCollection<DefaultFormatModel>();

            //DefaultFormatList.Add(new DefaultFormatModel { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string> { { "70.49N 40.32W", "Y#.##N X#.##E" }, { "70.49N,40.32W", "Y#.##N,X#.##E" } } });

            //LoadOutputConfiguration();
        }

        /// <summary>
        /// The bound list.
        /// </summary>
        public ObservableCollection<OutputCoordinateModel> OutputCoordinateList { get; set; }
        public ObservableCollection<DefaultFormatModel> DefaultFormatList { get; set; }

        #region relay commands
        [XmlIgnore]
        public RelayCommand DeleteCommand { get; set; }
        [XmlIgnore]
        public RelayCommand ConfigCommand { get; set; }
        [XmlIgnore]
        public RelayCommand ExpandCommand { get; set; }
        [XmlIgnore]
        public RelayCommand CopyCommand { get; set; }

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
                // lets make sure
                if (System.Windows.MessageBoxResult.Yes != System.Windows.MessageBox.Show(string.Format("Remove {0}?", name), "Confirm removal?", System.Windows.MessageBoxButton.YesNo))
                    return;

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
            var outputCoordItem = GetOCMByName(obj as string);

            var dlg = new EditOutputCoordinateView(this.DefaultFormatList, outputCoordItem);

            //dlg.Owner = System.Windows.Window.GetWindow(this.Data);

            if(dlg.ShowDialog() == true)
            {
            }

            SaveOutputConfiguration();
        }

        #endregion

        public void SaveOutputConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                XmlSerializer x = new XmlSerializer(GetType());
                XmlWriter writer = new XmlTextWriter(filename, Encoding.UTF8);

                x.Serialize(writer, this);
            }
            catch(Exception ex)
            {
            }
        }

        public void LoadOutputConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                    return;

                XmlSerializer x = new XmlSerializer(GetType());
                TextReader tr = new StreamReader(filename);
                var temp = x.Deserialize(tr) as OutputCoordinateViewModel;

                if (temp == null)
                    return;

                DefaultFormatList = temp.DefaultFormatList;
                OutputCoordinateList = temp.OutputCoordinateList;

                RaisePropertyChanged(() => DefaultFormatList);
                RaisePropertyChanged(() => OutputCoordinateList);
            }
            catch(Exception ex)
            {

            }
        }

        private string GetConfigFilename()
        {
            return this.GetType().Assembly.Location + ".config";
        }

        private OutputCoordinateModel GetOCMByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var item in OutputCoordinateList)
                {
                    if (item.Name == name)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
    }
}
