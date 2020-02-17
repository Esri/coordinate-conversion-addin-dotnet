/******************************************************************************* 
  * Copyright 2016 Esri 
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

using ProAppCoordConversionModule.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProAppCoordConversionModule.Models
{
    public class CoordinateConversionLibraryConfig : NotificationObject
    {
        public CoordinateConversionLibraryConfig()
        {
            OutputCoordinateList = new ObservableCollection<OutputCoordinateModel>();
            DefaultFormatList = new ObservableCollection<DefaultFormatModel>();

            LoadSomeDefaults();
        }

        public static CoordinateConversionLibraryConfig AddInConfig = new CoordinateConversionLibraryConfig();

        private CoordinateTypes displayCoordinateType = CoordinateTypes.Default;
        public CoordinateTypes DisplayCoordinateType
        {
            get { return displayCoordinateType; }
            set
            {
                displayCoordinateType = value;
                RaisePropertyChanged(() => DisplayCoordinateType);
            }
        }

        private bool isCustomFormat = false;
        public bool IsCustomFormat
        {
            get { return isCustomFormat; }
            set
            {
                isCustomFormat = value;
                RaisePropertyChanged(() => IsCustomFormat);
            }
        }

        private bool displayAmbiguousCoordsDlg = true;
        public bool DisplayAmbiguousCoordsDlg
        {
            get { return displayAmbiguousCoordsDlg; }
            set
            {
                displayAmbiguousCoordsDlg = value;
                RaisePropertyChanged(() => DisplayAmbiguousCoordsDlg);
            }
        }

        private bool _isLatLong = true;
        public bool isLatLong
        {
            get
            {
                return _isLatLong;
            }
            set
            {
                _isLatLong = value;
                RaisePropertyChanged(() => isLatLong);
            }
        }

        private bool showPlusForDirection = false;
        public bool ShowPlusForDirection
        {
            get { return showPlusForDirection; }
            set
            {
                showPlusForDirection = value;
                RaisePropertyChanged(() => ShowPlusForDirection);
            }
        }

        private bool showHyphenForDirection = false;
        public bool ShowHyphenForDirection
        {
            get { return showHyphenForDirection; }
            set
            {
                showHyphenForDirection = value;
                RaisePropertyChanged(() => ShowHyphenForDirection);

            }
        }

        private bool showHemisphereIndicator = false;
        public bool IsHemisphereIndicatorChecked
        {
            get { return showHemisphereIndicator; }
            set
            {
                showHemisphereIndicator = value;
                RaisePropertyChanged(() => IsHemisphereIndicatorChecked);

            }
        }

        private bool isPlusHyphenChecked;
        public bool IsPlusHyphenChecked
        {
            get { return isPlusHyphenChecked; }
            set
            {
                isPlusHyphenChecked = value;
                RaisePropertyChanged(() => IsPlusHyphenChecked);
            }
        }


        public string CategorySelection { get; set; }
        public string FormatSelection { get; set; }

        public ObservableCollection<OutputCoordinateModel> OutputCoordinateList { get; set; }
        public ObservableCollection<DefaultFormatModel> DefaultFormatList { get; set; }

        #region Public methods

        public void SaveConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                XmlSerializer x = new XmlSerializer(GetType());
                XmlWriter writer = new XmlTextWriter(filename, Encoding.UTF8);

                try
                {
                    x.Serialize(writer, this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void LoadConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                    return;

                XmlSerializer x = new XmlSerializer(GetType());
                TextReader tr = new StreamReader(filename);

                CoordinateConversionLibraryConfig temp = null;
                try
                {
                    temp = x.Deserialize(tr) as CoordinateConversionLibraryConfig;
                }
                catch
                {
                    /* Deserialize Failed */
                }
                finally
                {
                    tr.Close();
                }

                if (temp == null)
                    return;

                //DisplayCoordinateType = temp.DisplayCoordinateType;
                DisplayAmbiguousCoordsDlg = temp.DisplayAmbiguousCoordsDlg;
                OutputCoordinateList = temp.OutputCoordinateList;
                DefaultFormatList = temp.DefaultFormatList;
                ShowPlusForDirection = temp.ShowPlusForDirection;
                ShowHyphenForDirection = temp.ShowHyphenForDirection;
                IsHemisphereIndicatorChecked = temp.IsHemisphereIndicatorChecked;
                IsPlusHyphenChecked = temp.IsPlusHyphenChecked;
                IsHemisphereIndicatorChecked = temp.IsHemisphereIndicatorChecked;

                RaisePropertyChanged(() => IsPlusHyphenChecked);
                RaisePropertyChanged(() => IsHemisphereIndicatorChecked);
                RaisePropertyChanged(() => ShowPlusForDirection);
                RaisePropertyChanged(() => ShowHyphenForDirection);
                RaisePropertyChanged(() => OutputCoordinateList);
                RaisePropertyChanged(() => DefaultFormatList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #endregion Public methods

        #region Private methods

        public string GetConfigFolder()
        {
            // Use local user settings Pro folder 
            string configPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ArcGIS");

            // This should not happen, but use MyDocuments as backup just in case
            if (!System.IO.Directory.Exists(configPath))
                configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            return configPath;
        }

        private string GetConfigPath()
        {
            string assemblyName = GetType().Assembly.GetName().Name + ".config"; ;
            string configFolder = GetConfigFolder();
            string configPath = System.IO.Path.Combine(configFolder, assemblyName);

            return configPath;
        }

        private string GetConfigFilename()
        {
            return GetConfigPath();
        }

        private void LoadSomeDefaults()
        {
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.Default, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70.123456 -40.123456", Constants.DefaultCustomFormat } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70.123456N 40.123456W", Constants.DDCustomFormat } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DDM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49.1234'N 40° 18.1234'W", Constants.DDMCustomFormat } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DMS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49' 23.12\"N 40° 18' 45.12\"W", Constants.DMSCustomFormat } } });
            //DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.GARS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "221LW37", "X#YQK" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.MGRS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", Constants.MGRSCustomFormat } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.USNG, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", Constants.USNGCustomFormat } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.UTM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19F 414639 4428236", Constants.UTMCustomFormat } } });
        }

        #endregion Private methods
    }
}
