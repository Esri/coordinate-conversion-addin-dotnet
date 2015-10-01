using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Views;

namespace CoordinateToolLibrary.ViewModels
{
    public class CoordinateToolViewModel : BaseViewModel
    {
        public CoordinateToolViewModel()
        {
            OCView = new OutputCoordinateView();
        }

        public OutputCoordinateView OCView { get; set; }

        private CoordinateHandler ch = new CoordinateHandler();
        private CoordinateDD coord = new CoordinateDD(70.49, -40.32);
        private string inputCoordinate = "70.49N40.32W";

        public string InputCoordinate
        {
            get
            {
                return inputCoordinate;
            }
            set
            {
                inputCoordinate = value;
                CoordinateDD dd;
                if (ch.Parse(inputCoordinate, out dd))
                {
                    coord = dd;

                    UpdateOutputs();

                    RaisePropertyChanged(() => InputCoordinate);
                    RaisePropertyChanged(() => DD);
                    RaisePropertyChanged(() => DMS);
                    RaisePropertyChanged(() => AlternateDD);
                    RaisePropertyChanged(() => CustomDD);
                    RaisePropertyChanged(() => CustomDD2);
                    RaisePropertyChanged(() => CustomDMS);

                    HasInputError = false;
                }
                else
                {
                    HasInputError = true;
                }

                RaisePropertyChanged(() => HasInputError);
            }
        }

        private void UpdateOutputs()
        {
            foreach( var output in (OCView.DataContext as OutputCoordinateViewModel).OutputCoordinateList)
            {
                var props = new Dictionary<string, string>();

                switch(output.CType)
                {
                    case CoordinateType.DD:
                        output.OutputCoordinate = coord.ToString(output.Format, new CoordinateDDFormatter());
                        props.Add("Lat", coord.Lat.ToString());
                        props.Add("Lon", coord.Lon.ToString());
                        output.Props = props;
                        break;
                    case CoordinateType.DMS:
                        var dms = ch.GetDMS(coord);
                        output.OutputCoordinate = dms.ToString(output.Format, new CoordinateDMSFormatter());
                        props.Add("Lat D", dms.LatDegrees.ToString());
                        props.Add("Lat M", dms.LatMinutes.ToString());
                        props.Add("Lat S", Math.Truncate(dms.LatSeconds).ToString("##"));
                        props.Add("Lon D", dms.LonDegrees.ToString());
                        props.Add("Lon M", dms.LonMinutes.ToString());
                        props.Add("Lon S", Math.Truncate(dms.LonSeconds).ToString("##"));
                        output.Props = props;
                        break;
                    case CoordinateType.DDM:
                        break;
                    case CoordinateType.GARS:
                        break;
                    case CoordinateType.MGRS:
                        break;
                    case CoordinateType.USNG:
                        break;
                    case CoordinateType.UTM:
                        break;
                    default:
                        break;
                }
            }
        }

        public bool HasInputError
        { get; set; }

        public string DD
        {
            get
            {
                return String.Format("x = {0:#.0000} y = {1:#.0000}", coord.Lon, coord.Lat);
            }
        }

        public string DMS
        {
            get
            {
                var dms = ch.GetDMS(coord);
                return String.Format("{0}° {1}\' {2:##}\" {3} {4}° {5}\' {6:##}\" {7}", Math.Abs(dms.LatDegrees), dms.LatMinutes, dms.LatSeconds, dms.LatDegrees < 0 ? "S" : "N", Math.Abs(dms.LonDegrees), dms.LonMinutes, dms.LonSeconds, dms.LonDegrees < 0 ? "W" : "E");
            }
        }

        public string AlternateDD
        {
            get
            {
                return coord.ToString("DD");
            }
        }

        public string CustomDD
        {
            get
            {
                return coord.ToString("Y-+##.0000 X-+###.0000", new CoordinateDDFormatter());
            }
        }

        public string CustomDD2
        {
            get
            {
                return coord.ToString("Y##.0000 N X###.0000 E", new CoordinateDDFormatter());
            }
        }

        public string CustomDMS
        {
            get
            {
                var dms = ch.GetDMS(coord);
                return dms.ToString("A##°B##'C##\"N X###°Y##'Z##\"E", new CoordinateDMSFormatter());
            }
        }
    }
}
