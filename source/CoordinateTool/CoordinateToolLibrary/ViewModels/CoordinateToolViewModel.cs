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

            // set default CoordinateGetter
            coordinateGetter = new CoordinateGetBase();
        }

        public OutputCoordinateView OCView { get; set; }

        private CoordinateHandler ch = new CoordinateHandler();
        //private CoordinateDD coord = new CoordinateDD(70.49, -40.32);
        private string inputCoordinate = "70.49N40.32W";
        private CoordinateGetBase coordinateGetter;

        // InputCoordinate
        public string InputCoordinate
        {
            get
            {
                return inputCoordinate;
            }
            set
            {
                inputCoordinate = value;
                coordinateGetter.InputCoordinate = value;
                UpdateOutputs();
                //CoordinateDD dd;
                //if (ch.Parse(inputCoordinate, out dd))
                //{
                //    coord = dd;

                //    UpdateOutputs();

                //    RaisePropertyChanged(() => InputCoordinate);
                //    //RaisePropertyChanged(() => DD);
                //    //RaisePropertyChanged(() => DMS);
                //    //RaisePropertyChanged(() => AlternateDD);
                //    //RaisePropertyChanged(() => CustomDD);
                //    //RaisePropertyChanged(() => CustomDD2);
                //    //RaisePropertyChanged(() => CustomDMS);

                //    HasInputError = false;
                //}
                //else
                //{
                //    HasInputError = true;
                //}

                //RaisePropertyChanged(() => HasInputError);
            }
        }

        public void SetCoordinateGetter(CoordinateGetBase coordGetter)
        {
            coordinateGetter = coordGetter;
        }

        private void UpdateOutputs()
        {
            foreach( var output in (OCView.DataContext as OutputCoordinateViewModel).OutputCoordinateList)
            {
                var props = new Dictionary<string, string>();
                string coord = string.Empty;

                switch(output.CType)
                {
                    case CoordinateType.DD:
                        CoordinateDD cdd;
                        if (coordinateGetter.CanGetDD(out coord) &&
                            CoordinateDD.TryParse(coord, out cdd))
                        {
                            output.OutputCoordinate = cdd.ToString(output.Format, new CoordinateDDFormatter());
                            props.Add("Lat", cdd.Lat.ToString());
                            props.Add("Lon", cdd.Lon.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DMS:
                        CoordinateDMS cdms;
                        if (coordinateGetter.CanGetDMS(out coord) &&
                            CoordinateDMS.TryParse(coord, out cdms))
                        {
                            output.OutputCoordinate = cdms.ToString(output.Format, new CoordinateDMSFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add("Lat", cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                props.Add("Lon", cdms.ToString("X" + splits[1].Trim(), new CoordinateDMSFormatter()));
                            }
                            else
                            {
                                props.Add("Lat", cdms.ToString("A#°B#'C#.0\"N", new CoordinateDDFormatter()));
                                props.Add("Lon", cdms.ToString("X#°Y#'Z#\"E", new CoordinateDDFormatter()));
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DDM:

                        break;
                    case CoordinateType.GARS:
                        CoordinateGARS gars;
                        if(coordinateGetter.CanGetGARS(out coord) &&
                            CoordinateGARS.TryParse(coord, out gars))
                        {
                            output.OutputCoordinate = gars.ToString(output.Format, new CoordinateGARSFormatter());
                            props.Add("Lon", gars.LonBand.ToString());
                            props.Add("Lat", gars.LatBand);
                            props.Add("Quadrant", gars.Quadrant.ToString());
                            props.Add("Key", gars.Quadrant.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.MGRS:
                        CoordinateMGRS mgrs;
                        if(coordinateGetter.CanGetMGRS(out coord) &&
                            CoordinateMGRS.TryParse(coord, out mgrs))
                        {
                            output.OutputCoordinate = mgrs.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add("GZD", mgrs.GZD);
                            props.Add("Grid Sq", mgrs.GS);
                            props.Add("Easting", mgrs.Easting.ToString());
                            props.Add("Northing", mgrs.Northing.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.USNG:
                        break;
                    case CoordinateType.UTM:
                        CoordinateUTM utm;
                        if(coordinateGetter.CanGetUTM(out coord) &&
                            CoordinateUTM.TryParse(coord, out utm))
                        {
                            output.OutputCoordinate = utm.ToString(output.Format, new CoordinateUTMFormatter());
                            props.Add("Zone", utm.Zone.ToString() + utm.Hemi);
                            props.Add("Easting", utm.Easting.ToString());
                            props.Add("Northing", utm.Northing.ToString());
                            output.Props = props;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public bool HasInputError
        { get; set; }

        #region OLD CODE
        //public string DD
        //{
        //    get
        //    {
        //        return String.Format("x = {0:#.0000} y = {1:#.0000}", coord.Lon, coord.Lat);
        //    }
        //}

        //public string DMS
        //{
        //    get
        //    {
        //        var dms = ch.GetDMS(coord);
        //        return String.Format("{0}° {1}\' {2:##}\" {3} {4}° {5}\' {6:##}\" {7}", Math.Abs(dms.LatDegrees), dms.LatMinutes, dms.LatSeconds, dms.LatDegrees < 0 ? "S" : "N", Math.Abs(dms.LonDegrees), dms.LonMinutes, dms.LonSeconds, dms.LonDegrees < 0 ? "W" : "E");
        //    }
        //}

        //public string AlternateDD
        //{
        //    get
        //    {
        //        return coord.ToString("DD");
        //    }
        //}

        //public string CustomDD
        //{
        //    get
        //    {
        //        return coord.ToString("Y-+##.0000 X-+###.0000", new CoordinateDDFormatter());
        //    }
        //}

        //public string CustomDD2
        //{
        //    get
        //    {
        //        return coord.ToString("Y##.0000 N X###.0000 E", new CoordinateDDFormatter());
        //    }
        //}

        //public string CustomDMS
        //{
        //    get
        //    {
        //        var dms = ch.GetDMS(coord);
        //        return dms.ToString("A##°B##'C##\"N X###°Y##'Z##\"E", new CoordinateDMSFormatter());
        //    }
        //}
        #endregion
    }
}
