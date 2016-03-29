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
        }

        private CoordinateTypes displayCoordinateType = CoordinateTypes.None;
        public CoordinateTypes DisplayCoordinateType
        {
            get { return displayCoordinateType; }
            set
            {
                displayCoordinateType = value;
                RaisePropertyChanged(() => DisplayCoordinateType);
                Mediator.NotifyColleagues(Constants.DISPLAY_COORDINATE_TYPE_CHANGED, null);
            }
        }

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
                // do nothing
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
                var temp = x.Deserialize(tr) as CoordinateConversionLibraryConfig;

                if (temp == null)
                    return;

                DisplayCoordinateType = temp.DisplayCoordinateType;
            }
            catch (Exception ex)
            {
                // do nothing
            }
        }

        #endregion Public methods
    
        #region Private methods

        private string GetConfigFilename()
        {
            return this.GetType().Assembly.Location + ".config";
        }

        #endregion Private methods
    }
}
