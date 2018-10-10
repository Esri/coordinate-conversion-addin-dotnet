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

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using CoordinateConversionLibrary.Helpers;

namespace CoordinateConversionLibrary.Models
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

                x.Serialize(writer, this);
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

        private string GetConfigFilename()
        {
            return this.GetType().Assembly.Location + ".config";
        }

        private void LoadSomeDefaults()
        {
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.Default, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70.123456 -40.123456", "Y0.0##### X0.0#####" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DD, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70.123456N 40.123456W", "Y0.0#####N X0.0#####E" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DDM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49.1234'N 40° 18.1234'W", "A0° B0.0###'N X0° Y0.0###'E" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.DMS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "70° 49' 23.12\"N 40° 18' 45.12\"W", "A0° B0' C0.0#\"N X0° Y0' Z0.0#\"E" } } });
            //DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.GARS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "221LW37", "X#YQK" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.MGRS, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", "ZSX00000Y00000" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.USNG, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19TDE1463928236", "ZSX00000Y00000" } } });
            DefaultFormatList.Add(new DefaultFormatModel() { CType = CoordinateType.UTM, DefaultNameFormatDictionary = new SerializableDictionary<string, string>() { { "19F 414639 4428236", "Z#B X0 Y0" } } });
        }

        #endregion Private methods
    }
}
