using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Models;

namespace CoordinateToolLibrary.ViewModels
{
    public class CoordinateToolViewModel : BaseViewModel
    {
        public CoordinateToolViewModel()
        { }

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
