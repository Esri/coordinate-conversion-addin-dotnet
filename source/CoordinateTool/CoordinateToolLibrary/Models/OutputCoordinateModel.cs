using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoordinateToolLibrary.Helpers;
using System.Xml.Serialization;

namespace CoordinateToolLibrary.Models
{
    public class OutputCoordinateModel : NotificationObject
    {
        public OutputCoordinateModel()
        {
            DVisibility = Visibility.Collapsed;
            _props = new Dictionary<string, string>();
        }

        #region Details Visibility
        [XmlIgnore]
        public Visibility DVisibility { get; set; }
        #endregion

        #region Name

        private string name = String.Empty;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        #region Props
        private Dictionary<string, string> _props;
        [XmlIgnore]
        public Dictionary<string, string> Props 
        {
            get
            {
                return _props;
            }
            set
            {
                _props = value;
                RaisePropertyChanged(() => Props);
            }
        }
        #endregion

        #region OutputCoordinate

        private string outputCoordinate = String.Empty;

        [XmlIgnore]
        public string OutputCoordinate
        {
            get { return outputCoordinate; }
            set
            {
                outputCoordinate = value;
                RaisePropertyChanged(() => OutputCoordinate);
            }
        }

        #endregion

        #region CType
        public CoordinateType CType { get; set; }
        #endregion

        #region Format
        private string format = "Y-+##.0000 X-+###.0000";
        public string Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                RaisePropertyChanged(() => Format);
            }
        }
        #endregion Format

        private int srFactoryCode = 4326;
        public int SRFactoryCode
        {
            get
            {
                return srFactoryCode;
            }
            set
            {
                if (srFactoryCode != value)
                {
                    srFactoryCode = value;
                    RaisePropertyChanged(() => SRFactoryCode);
                }
            }
        }

        private string srName = "WGS84";
        public string SRName
        {
            get
            {
                return srName;
            }

            set
            {
                if(srName != value)
                {
                    srName = value;
                    RaisePropertyChanged(() => SRName);
                }
            }
        }

        #region Methods
        public void ToggleVisibility()
        {
            if (this.DVisibility == Visibility.Collapsed)
                this.DVisibility = Visibility.Visible;
            else
                this.DVisibility = Visibility.Collapsed;

            RaisePropertyChanged(() => DVisibility);
        }
        #endregion
    }
}
