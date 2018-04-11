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
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Views;

namespace CoordinateConversionLibrary.ViewModels
{
    public class OutputCoordinateViewModel : BaseViewModel
    {
        public OutputCoordinateViewModel()
        {
            ConfigCommand = new RelayCommand(OnConfigCommand);
            ExpandCommand = new RelayCommand(OnExpandCommand);
            DeleteCommand = new RelayCommand(OnDeleteCommand);
            CopyCommand = new RelayCommand(OnCopyCommand);
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            CopyAllCommand = new RelayCommand(OnCopyAllCommand);

            // set default CoordinateGetter
            coordinateGetter = new CoordinateGetBase();

            /* KG - Commented since add new output coordinate and copy all coordinate outputs buttons are now in the OutputCoordinateView.xaml
            Mediator.Register(CoordinateConversionLibrary.Constants.AddNewOutputCoordinate, OnAddNewOutputCoordinate); 
            Mediator.Register(CoordinateConversionLibrary.Constants.CopyAllCoordinateOutputs, OnCopyAllCoordinateOutputs);
             * */
            Mediator.Register(CoordinateConversionLibrary.Constants.SetCoordinateGetter, OnSetCoordinateGetter);
            Mediator.Register(CoordinateConversionLibrary.Constants.RequestOutputUpdate, OnOutputUpdate);
            Mediator.Register(CoordinateConversionLibrary.Constants.ClearOutputCoordinates, OnClearOutputs);

            //for testing without a config file, init a few sample items
            //OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            ////var tempProps = new Dictionary<string, string>() { { "Lat", "70.49N" }, { "Lon", "40.32W" } };
            ////var mgrsProps = new Dictionary<string, string>() { { "GZone", "17T" }, { "GSquare", "NE" }, { "Northing", "86309" }, { "Easting", "77770" } };
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DD", CType = CoordinateType.DD, OutputCoordinate = "70.49N 40.32W" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DMS", CType = CoordinateType.DMS, OutputCoordinate = "40°26'46\"N,79°58'56\"W", Format = "A0°B0'C0\"N X0°Y0'Z0\"E" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "MGRS", CType = CoordinateType.MGRS, OutputCoordinate = @"", Format = "Z S X00000 Y00000" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "UTM", CType = CoordinateType.UTM, OutputCoordinate = @"", Format = "Z#B X0 Y0" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "GARS", CType = CoordinateType.GARS, OutputCoordinate = @"", Format = "X#YQK" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "USNG", CType = CoordinateType.USNG, OutputCoordinate = @"", Format = "Z S X0 Y0" });
            //OutputCoordinateList.Add(new OutputCoordinateModel { Name = "DDM", CType = CoordinateType.DDM, OutputCoordinate = @"", Format = "A0 B0.0### N X0 Y0.0### E" });

            //DefaultFormatList = new ObservableCollection<DefaultFormatModel>();

            //DefaultFormatList.Add(new DefaultFormatModel { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string> { { "70.49N 40.32W", "Y0.0#N X0.0#E" }, { "70.49N,40.32W", "Y0.0#N,X0.0#E" } } });

            configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionLibraryConfig.AddInConfig)
            .RegisterHandler(n => n.OutputCoordinateList, n => RaisePropertyChanged(() => OutputCoordinateList));
        }

        PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

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

            var dlg = new EditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, GetInUseNames(), new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name = outputCoordItem.Name, SRName = outputCoordItem.SRName, SRFactoryCode = outputCoordItem.SRFactoryCode });

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

        // Call AddNewOutputCoordinate event
        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            this.OnAddNewOutputCoordinate(new OutputCoordinateModel()
            {
                Name = CoordinateType.DD.ToString(),
                CType = CoordinateType.DD,
                Format = "Y0.0#####N X0.0#####E"
            });
        }

        // Call CopyAllCoordinateOutputs event
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

        private void OnDeleteCommand(object obj)
        {
            var name = obj as string;

            if (!string.IsNullOrEmpty(name))
            {
                // lets make sure
                if (System.Windows.MessageBoxResult.Yes != System.Windows.MessageBox.Show(string.Format(Properties.Resources.FormattedRemove, name), Properties.Resources.LabelConfirmRemoval, System.Windows.MessageBoxButton.YesNo))
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
            var dlg = new EditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, InUseNames,
                new OutputCoordinateModel()
                {
                    CType = outputCoordItem.CType,
                    Format = outputCoordItem.Format,
                    Name = outputCoordItem.Name,
                    SRName = outputCoordItem.SRName,
                    SRFactoryCode = outputCoordItem.SRFactoryCode
                });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
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
                            CoordinateDD.TryParse(coord, out cdd))
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
                            CoordinateDMS.TryParse(coord, out cdms))
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
                            CoordinateDDM.TryParse(coord, out ddm))
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
