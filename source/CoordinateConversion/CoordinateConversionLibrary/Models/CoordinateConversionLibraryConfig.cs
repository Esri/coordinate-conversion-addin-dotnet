using CoordinateConversionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateConversionLibrary.Models
{
    public class CoordinateConversionLibraryConfig : NotificationObject
    {
        public CoordinateConversionLibraryConfig()
        {

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
    }
}
