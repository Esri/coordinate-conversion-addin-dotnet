using CoordinateConversionLibrary;
using CoordinateConversionLibrary.ViewModels;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArcMapAddinCoordinateConversion.ValueConverters
{
    [ValueConversion(typeof(IPoint), typeof(String))]
    public class IPointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "NA";

            var point = value as IPoint;
            if (point == null)
                return "NA";

            var result = string.Format("{0:0.0} {1:0.0}", point.Y, point.X);

            if (point.SpatialReference == null)
                return result;

            var cn = point as IConversionNotation;
            if (cn != null)
            {
                switch (CoordinateConversionViewModel.AddInConfig.DisplayCoordinateType)
                {
                    case CoordinateTypes.DD:
                        result = cn.GetDDFromCoords(6);
                        break;
                    case CoordinateTypes.DDM:
                        result = cn.GetDDMFromCoords(4);
                        break;
                    case CoordinateTypes.DMS:
                        result = cn.GetDMSFromCoords(2);
                        break;
                    case CoordinateTypes.GARS:
                        result = cn.GetGARSFromCoords();
                        break;
                    case CoordinateTypes.MGRS:
                        result = cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
                        break;
                    case CoordinateTypes.USNG:
                        result = cn.GetUSNGFromCoords(5, true, true);
                        break;
                    case CoordinateTypes.UTM:
                        result = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces | esriUTMConversionOptionsEnum.esriUTMUseNS);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
