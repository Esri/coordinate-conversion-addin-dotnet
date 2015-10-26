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

            Mediator.Register("AddNewOutputCoordinate", OnAddNewOutputCoordinate);
            Mediator.Register("COPY_ALL_COORDINATE_OUTPUTS", OnCopyAllCoordinateOutputs);

            OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            DefaultFormatList = new ObservableCollection<DefaultFormatModel>();

            //init a few sample items
            //OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            ////var tempProps = new Dictionary<string, string>() { { "Lat", "70.49N" }, { "Lon", "40.32W" } };
            ////var mgrsProps = new Dictionary<string, string>() { { "GZone", "17T" }, { "GSquare", "NE" }, { "Northing", "86309" }, { "Easting", "77770" } };
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DD", CType = CoordinateType.DD, OutputCoordinate = "70.49N 40.32W" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DMS", CType = CoordinateType.DMS, OutputCoordinate = "40°26'46\"N,79°58'56\"W", Format = "A#°B0'C0\"N X#°Y0'Z0\"E" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "MGRS", CType = CoordinateType.MGRS, OutputCoordinate = @"", Format = "Z S X# Y#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "UTM", CType = CoordinateType.UTM, OutputCoordinate = @"", Format = "Z#H X# Y#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "GARS", CType = CoordinateType.GARS, OutputCoordinate = @"", Format = "X#YQK" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "USNG", CType = CoordinateType.USNG, OutputCoordinate = @"", Format = "Z S X# Y#" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DDM", CType = CoordinateType.DDM, OutputCoordinate = @"", Format = "A# B#.#### N X# Y#.#### E" });

            //DefaultFormatList = new ObservableCollection<DefaultFormatModel>();

            //DefaultFormatList.Add(new DefaultFormatModel { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string> { { "70.49N 40.32W", "Y#.##N X#.##E" }, { "70.49N,40.32W", "Y#.##N,X#.##E" } } });

            //LoadOutputConfiguration();
        }

        private void OnCopyAllCoordinateOutputs(object obj)
        {
            var sb = new StringBuilder();

            foreach(var output in OutputCoordinateList)
            {
                sb.AppendLine(output.OutputCoordinate);
            }

            if(sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }

        private void OnAddNewOutputCoordinate(object obj)
        {
            var outputCoordItem = obj as OutputCoordinateModel;

            if (outputCoordItem == null)
                return;

            var dlg = new EditOutputCoordinateView(this.DefaultFormatList, GetInUseNames(), new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name= outputCoordItem.Name });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            vm.WindowTitle = "Add New Output Coordinate";
            
            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Format = vm.Format;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                outputCoordItem.Name = vm.OutputCoordItem.Name;

                OutputCoordinateList.Add(outputCoordItem);
                Mediator.NotifyColleagues("UpdateOutputRequired", null);
                SaveOutputConfiguration();
            }
        }

        private List<string> GetInUseNames()
        {
            return OutputCoordinateList.Select(oc => oc.Name).ToList();
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
                        SaveOutputConfiguration();
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
            var InUseNames = GetInUseNames();
            InUseNames.Remove(outputCoordItem.Name);
            var dlg = new EditOutputCoordinateView(this.DefaultFormatList, InUseNames, new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name = outputCoordItem.Name });

            //dlg.Owner = System.Windows.Window.GetWindow(this.Data);

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            vm.WindowTitle = "Edit Output Coordinate";

            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Name = vm.OutputCoordItem.Name;
                outputCoordItem.Format = vm.Format;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                Mediator.NotifyColleagues("UpdateOutputRequired", null);
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
                {
                    LoadSomeDefaults();
                    return;
                }

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

        private void LoadSomeDefaults()
        {
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70.49N 40.32W", "Y#.##N X#.##E" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DDM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49.12'N 40° 18.32'W", "A#° B#.##'N X#° Y#.##'E" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DMS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49' 23.2\"N 40° 18' 45.4\"W", "A#° B#' C#.#\"N X#° Y#' Z#.#E\"" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.GARS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "221LW37", "X#YQK" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.MGRS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", "ZSX#Y#" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.USNG, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", "ZSX#Y#" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.UTM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19N 414639m 4428236m", "Z#H X#m Y#m" } } });
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
