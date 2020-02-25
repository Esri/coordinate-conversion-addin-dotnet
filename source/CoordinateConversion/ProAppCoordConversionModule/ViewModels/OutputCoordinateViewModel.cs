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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ProAppCoordConversionModule.Models;
using ProAppCoordConversionModule.Helpers;
using System.IO;
using System.Globalization;
using Microsoft.Win32;
using System.Data;
using System.Windows;
using ProAppCoordConversionModule.Views;

namespace ProAppCoordConversionModule.ViewModels
{
    public class OutputCoordinateViewModel : NotificationObject
    {
        public OutputCoordinateViewModel()
        {
            ConfigCommand = new RelayCommand(OnConfigCommand);
            ExpandCommand = new RelayCommand(OnExpandCommand);
            DeleteCommand = new RelayCommand(OnDeleteCommand);
            CopyCommand = new RelayCommand(OnCopyCommand);
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            CopyAllCommand = new RelayCommand(OnCopyAllCommand);
            ImportButtonCommand = new RelayCommand(OnImportButtonCommand);
            ExportButtonCommand = new RelayCommand(OnExportButtonCommand);
            ResetButtonCommand = new RelayCommand(OnResetButtonCommand);

            // set default CoordinateGetter
            coordinateGetter = new CoordinateGetBase();

            SetCoordinateCommand = new RelayCommand(OnSetCoordinateGetter);
            RequestOutputCommand = new RelayCommand(OnOutputUpdate);
            ClearOutputCoordinates = new RelayCommand(OnClearOutputs);

            configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
            .RegisterHandler(n => n.OutputCoordinateList, n => RaisePropertyChanged(() => OutputCoordinateList));
        }

        PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        string headers = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}"
                        , "CType", "DVisibility", "Format", "Name", "OutputCoordinate", "SRFactoryCode", "SRName");
        private CoordinateGetBase coordinateGetter;

        private void OnClearOutputs(object obj)
        {
            ClearOutputs();
        }

        private void OnCopyAllCoordinateOutputs(object obj)
        {
            var sb = new StringBuilder();

            string inputCoordinate = obj as string;

            if (!string.IsNullOrWhiteSpace(inputCoordinate))
                sb.AppendLine(inputCoordinate);

            foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
            {
                sb.AppendLine(output.OutputCoordinate);
            }

            if (sb.Length > 0)
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }

        public virtual void OnAddNewOutputCoordinate(object obj)
        {
            var outputCoordItem = obj as OutputCoordinateModel;
            if (outputCoordItem == null)
                return;

            var dlg = new ProEditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, GetInUseNames(), new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name = outputCoordItem.Name, SRName = outputCoordItem.SRName, SRFactoryCode = outputCoordItem.SRFactoryCode });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            vm.WindowTitle = Properties.Resources.TitleAddNewOutputCoordinate;

            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Format = vm.Format;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                outputCoordItem.Name = vm.OutputCoordItem.Name;
                outputCoordItem.SRFactoryCode = vm.OutputCoordItem.SRFactoryCode;
                outputCoordItem.SRName = vm.OutputCoordItem.SRName;

                CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Add(outputCoordItem);
                UpdateOutputs();
                CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
            }
        }

        private void OnSetCoordinateGetter(object obj)
        {
            coordinateGetter = obj as CoordinateGetBase;
        }

        private void OnOutputUpdate(object obj)
        {
            UpdateOutputs();
        }

        public List<string> GetInUseNames()
        {
            return CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Select(oc => oc.Name).ToList();
        }

        /// <summary>
        /// The bound list.
        /// </summary>
        public ObservableCollection<OutputCoordinateModel> OutputCoordinateList
        {
            get { return CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList; }
        }

        #region relay commands
        [XmlIgnore]
        public RelayCommand DeleteCommand { get; set; }
        [XmlIgnore]
        public RelayCommand ConfigCommand { get; set; }
        [XmlIgnore]
        public RelayCommand ExpandCommand { get; set; }
        [XmlIgnore]
        public RelayCommand CopyCommand { get; set; }
        [XmlIgnore]
        public RelayCommand AddNewOCCommand { get; set; }
        [XmlIgnore]
        public RelayCommand CopyAllCommand { get; set; }
        public RelayCommand ImportButtonCommand { get; set; }
        public RelayCommand ExportButtonCommand { get; set; }
        public RelayCommand ResetButtonCommand { get; set; }
        public RelayCommand SetCoordinateCommand { get; set; }
        public RelayCommand RequestOutputCommand { get; set; }
        public RelayCommand ClearOutputCoordinates { get; set; }

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            this.OnAddNewOutputCoordinate(new OutputCoordinateModel()
            {
                Name = CoordinateType.DD.ToString(),
                CType = CoordinateType.DD,
                Format = Constants.DDCustomFormat
            });
        }
                
        private void OnCopyAllCommand(object obj)
        {
            this.OnCopyAllCoordinateOutputs(obj);
        }

        // copy parameter to clipboard
        private void OnCopyCommand(object obj)
        {
            var coord = obj as string;

            if (!string.IsNullOrWhiteSpace(coord))
            {
                // copy to clipboard
                System.Windows.Clipboard.SetText(coord);
            }
        }

        public virtual void OnDeleteCommand(object obj)
        {
            var name = obj as string;

            if (!string.IsNullOrEmpty(name))
            {
                // lets make sure
                if (System.Windows.MessageBoxResult.Yes != ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(string.Format(Properties.Resources.FormattedRemove, name), Properties.Resources.LabelConfirmRemoval, System.Windows.MessageBoxButton.YesNo))
                    return;

                foreach (var item in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    if (item.Name == name)
                    {
                        CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Remove(item);
                        CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
                        return;
                    }
                }
            }
        }

        private void OnExpandCommand(object obj)
        {
            var name = obj as string;

            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (var item in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    if (item.Name == name)
                    {
                        item.ToggleVisibility();
                        return;
                    }
                }
            }
        }

        public virtual void OnConfigCommand(object obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj as string))
                return;

            var outputCoordItem = GetOCMByName(obj as string);
            var InUseNames = GetInUseNames();
            InUseNames.Remove(outputCoordItem.Name);
            var dlg = new ProEditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, InUseNames,
                new OutputCoordinateModel()
                {
                    CType = outputCoordItem.CType,
                    Format = outputCoordItem.Format,
                    Name = outputCoordItem.Name,
                    SRName = outputCoordItem.SRName,
                    SRFactoryCode = outputCoordItem.SRFactoryCode
                });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            if (vm == null)
                return;

            vm.WindowTitle = Properties.Resources.TitleEditOutputCoordinate;

            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Name = vm.OutputCoordItem.Name;
                outputCoordItem.Format = vm.Format;
                outputCoordItem.SRFactoryCode = vm.OutputCoordItem.SRFactoryCode;
                outputCoordItem.SRName = vm.OutputCoordItem.SRName;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                UpdateOutputs();
            }

            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
        }


        public virtual void OnImportButtonCommand(object obj)
        {
            try
            {
                var openDialog = new OpenFileDialog();
                openDialog.Title = "Open File";
                openDialog.CheckFileExists = true;
                openDialog.CheckPathExists = true;
                openDialog.Filter = "csv files|*.csv";
                if (openDialog.ShowDialog() == true)
                {
                    var filePath = openDialog.FileName;
                    var s = File.ReadAllText(filePath);
                    var dt = new DataTable();

                    string[] tableData = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var col = from cl in tableData[0].Split(",".ToCharArray())
                              select new DataColumn(cl);
                    dt.Columns.AddRange(col.ToArray());

                    (from st in tableData.Skip(1)
                     select dt.Rows.Add(st.Split(",".ToCharArray()))).ToList();

                    var temp = dt;
                    foreach (DataRow item in dt.Rows)
                    {
                        string itemName = Convert.ToString(item["Name"]);
                        var coordFormats = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Where(x => x.Name == itemName).ToList();
                        if (coordFormats.Count > 0)
                        {
                            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Where(x => x.Name == itemName).Select(x =>
                            {
                                x.CType = (CoordinateType)Enum.Parse(typeof(CoordinateType), Convert.ToString(item["Ctype"]));
                                x.DVisibility = (Visibility)Enum.Parse(typeof(Visibility), Convert.ToString(item["DVisibility"]));
                                x.Format = Convert.ToString(item["Format"]);
                                x.OutputCoordinate = Convert.ToString(item["OutputCoordinate"]);
                                x.SRFactoryCode = Convert.ToInt32(item["SRFactoryCode"]);
                                x.SRName = Convert.ToString(item["SRName"]);
                                return x;
                            }).ToList();
                        }
                        else
                        {
                            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Add(
                            new OutputCoordinateModel()
                            {
                                CType = (CoordinateType)Enum.Parse(typeof(CoordinateType), Convert.ToString(item["Ctype"])),
                                DVisibility = (Visibility)Enum.Parse(typeof(Visibility), Convert.ToString(item["DVisibility"])),
                                Format = Convert.ToString(item["Format"]),
                                Name = itemName,
                                OutputCoordinate = Convert.ToString(item["OutputCoordinate"]),
                                SRFactoryCode = Convert.ToInt32(item["SRFactoryCode"]),
                                SRName = Convert.ToString(item["SRName"])
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Something went wrong.");
            }
        }

        public virtual void OnResetButtonCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList = new System.Collections.ObjectModel.ObservableCollection<OutputCoordinateModel>();
            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
            RaisePropertyChanged(() => CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList);
        }

        public virtual void OnExportButtonCommand(object obj)
        {
            try
            {
                if (CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Count == 0)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No data available");
                    return;
                }
                var saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save File";
                saveDialog.Filter = "csv files|*.csv";
                saveDialog.ShowDialog();
                var filePath = saveDialog.FileName;
                using (var file = File.CreateText(filePath))
                {
                    var list = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList
                        .Select(x => new
                        {
                            CType = x.CType,
                            DVisibility = x.DVisibility,
                            Format = x.Format,
                            Name = x.Name,
                            OutputCoordinate = x.OutputCoordinate,
                            SRFactoryCode = x.SRFactoryCode,
                            SRName = x.SRName
                        });
                    file.Write(headers);
                    file.WriteLine();
                    foreach (var arr in list)
                    {
                        if (arr == null) continue;
                        var str = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}"
                            , arr.CType, arr.DVisibility, arr.Format, arr.Name, arr.OutputCoordinate, arr.SRFactoryCode, arr.SRName);
                        file.Write(str);
                        file.WriteLine();
                    }
                }
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("File Exported to " + filePath);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public OutputCoordinateModel GetOCMByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var item in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
                {
                    if (item.Name == name)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private void ClearOutputs()
        {
            foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
            {
                output.OutputCoordinate = "";
                output.Props.Clear();
            }
        }

        public void UpdateOutputs()
        {
            foreach (var output in CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList)
            {
                var props = new Dictionary<string, string>();
                string coord = string.Empty;

                switch (output.CType)
                {
                    case CoordinateType.DD:
                        CoordinateDD cdd;
                        if (coordinateGetter.CanGetDD(output.SRFactoryCode, out coord) &&
                            CoordinateDD.TryParse(coord, out cdd, true))
                        {
                            output.OutputCoordinate = cdd.ToString(output.Format, new CoordinateDDFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add(Properties.Resources.StringLat, cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                props.Add(Properties.Resources.StringLon, cdd.ToString("X" + splits[1].Trim(), new CoordinateDDFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                    props.Add(Properties.Resources.StringLat, cdd.ToString("Y" + splits[1].Trim(), new CoordinateDDFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, cdd.Lat.ToString());
                                    props.Add(Properties.Resources.StringLon, cdd.Lon.ToString());
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DMS:
                        CoordinateDMS cdms;
                        if (coordinateGetter.CanGetDMS(output.SRFactoryCode, out coord) &&
                            CoordinateDMS.TryParse(coord, out cdms, true))
                        {
                            output.OutputCoordinate = cdms.ToString(output.Format, new CoordinateDMSFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add(Properties.Resources.StringLat, cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                props.Add(Properties.Resources.StringLon, cdms.ToString("X" + splits[1].Trim(), new CoordinateDMSFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                    props.Add(Properties.Resources.StringLat, cdms.ToString("Y" + splits[1].Trim(), new CoordinateDMSFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, cdms.ToString("A0°B0'C0.0\"N", new CoordinateDMSFormatter()));
                                    props.Add(Properties.Resources.StringLon, cdms.ToString("X0°Y0'Z0.0\"E", new CoordinateDMSFormatter()));
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DDM:
                        CoordinateDDM ddm;
                        if (coordinateGetter.CanGetDDM(output.SRFactoryCode, out coord) &&
                            CoordinateDDM.TryParse(coord, out ddm, true))
                        {
                            output.OutputCoordinate = ddm.ToString(output.Format, new CoordinateDDMFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add(Properties.Resources.StringLat, ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                props.Add(Properties.Resources.StringLon, ddm.ToString("X" + splits[1].Trim(), new CoordinateDDMFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                    props.Add(Properties.Resources.StringLat, ddm.ToString("Y" + splits[1].Trim(), new CoordinateDDMFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, ddm.ToString("A0°B0.0#####'N", new CoordinateDDMFormatter()));
                                    props.Add(Properties.Resources.StringLon, ddm.ToString("X0°Y0.0#####'E", new CoordinateDDMFormatter()));
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.GARS:
                        CoordinateGARS gars;
                        if (coordinateGetter.CanGetGARS(output.SRFactoryCode, out coord) &&
                            CoordinateGARS.TryParse(coord, out gars))
                        {
                            output.OutputCoordinate = gars.ToString(output.Format, new CoordinateGARSFormatter());
                            props.Add(Properties.Resources.StringLon, gars.LonBand.ToString());
                            props.Add(Properties.Resources.StringLat, gars.LatBand);
                            props.Add(Properties.Resources.StringQuadrant, gars.Quadrant.ToString());
                            props.Add(Properties.Resources.StringKey, gars.Key.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.MGRS:
                        CoordinateMGRS mgrs;
                        if (coordinateGetter.CanGetMGRS(output.SRFactoryCode, out coord) &&
                            CoordinateMGRS.TryParse(coord, out mgrs))
                        {
                            output.OutputCoordinate = mgrs.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add(Properties.Resources.StringGZD, mgrs.GZD);
                            props.Add(Properties.Resources.StringGridSq, mgrs.GS);
                            props.Add(Properties.Resources.StringEasting, mgrs.Easting.ToString("00000"));
                            props.Add(Properties.Resources.StringNorthing, mgrs.Northing.ToString("00000"));
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.USNG:
                        CoordinateUSNG usng;
                        if (coordinateGetter.CanGetUSNG(output.SRFactoryCode, out coord) &&
                            CoordinateUSNG.TryParse(coord, out usng))
                        {
                            output.OutputCoordinate = usng.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add(Properties.Resources.StringGZD, usng.GZD);
                            props.Add(Properties.Resources.StringGridSq, usng.GS);
                            props.Add(Properties.Resources.StringEasting, usng.Easting.ToString("00000"));
                            props.Add(Properties.Resources.StringNorthing, usng.Northing.ToString("00000"));
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.UTM:
                        CoordinateUTM utm;
                        if (coordinateGetter.CanGetUTM(output.SRFactoryCode, out coord) &&
                            CoordinateUTM.TryParse(coord, out utm))
                        {
                            output.OutputCoordinate = utm.ToString(output.Format, new CoordinateUTMFormatter());
                            var usingBand = output.Format.Contains("B");
                            props.Add(Properties.Resources.StringZone, utm.Zone.ToString() + (usingBand ? utm.Band : utm.Hemi));
                            props.Add(Properties.Resources.StringEasting, utm.Easting.ToString("000000"));
                            props.Add(Properties.Resources.StringNorthing, utm.Northing.ToString("0000000"));
                            output.Props = props;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
