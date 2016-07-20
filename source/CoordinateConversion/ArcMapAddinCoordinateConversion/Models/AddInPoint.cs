using ESRI.ArcGIS.Geometry;
using CoordinateConversionLibrary.Helpers;
using ArcMapAddinCoordinateConversion.ValueConverters;

namespace ArcMapAddinCoordinateConversion.Models
{
    public class AddInPoint : NotificationObject
    {
        public AddInPoint()
        {

        }

        private IPointToStringConverter pointConverter = new IPointToStringConverter();

        private IPoint point = null;
        public IPoint Point
        {
            get
            {
                return point;
            }
            set
            {
                point = value;

                RaisePropertyChanged(() => Point);
                RaisePropertyChanged(() => Text);
            }
        }
        public string Text
        {
            get { return pointConverter.Convert(point as object, typeof(string), null, null) as string; }
        }

        private string guid = string.Empty;
        public string GUID
        {
            get
            {
                return guid;
            }
            set
            {
                guid = value;
                RaisePropertyChanged(() => GUID);
            }
        }
        /// <summary>
        /// Property used to determine if it is selected in the listbox
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
