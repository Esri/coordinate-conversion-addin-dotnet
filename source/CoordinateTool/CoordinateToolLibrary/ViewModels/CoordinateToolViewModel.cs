using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Views;
using CoordinateToolLibrary.Helpers;

namespace CoordinateToolLibrary.ViewModels
{
    public class CoordinateToolViewModel : BaseViewModel
    {
        public CoordinateToolViewModel()
        {
            OCView = new OutputCoordinateView();

            // set default CoordinateGetter
            coordinateGetter = new CoordinateGetBase();

            Mediator.Register(CoordinateToolLibrary.Constants.RequestOutputUpdate, OnUpdateOutputs);
            Mediator.Register(CoordinateToolLibrary.Constants.SelectSpatialReference, OnSelectSpatialReference);
        }

        public OutputCoordinateView OCView { get; set; }
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
            }
        }

        public void SetCoordinateGetter(CoordinateGetBase coordGetter)
        {
            coordinateGetter = coordGetter;
        }

        private void OnUpdateOutputs(object obj)
        {
            UpdateOutputs();
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
                        if (coordinateGetter.CanGetDD(output.SRFactoryCode, out coord) &&
                            CoordinateDD.TryParse(coord, out cdd))
                        {
                            output.OutputCoordinate = cdd.ToString(output.Format, new CoordinateDDFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add("Lat", cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                props.Add("Lon", cdd.ToString("X" + splits[1].Trim(), new CoordinateDDFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add("Lon", cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                    props.Add("Lat", cdd.ToString("Y" + splits[1].Trim(), new CoordinateDDFormatter()));
                                }
                                else
                                {
                                    props.Add("Lat", cdd.Lat.ToString());
                                    props.Add("Lon", cdd.Lon.ToString());
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DMS:  
                        CoordinateDMS cdms;
                        if (coordinateGetter.CanGetDMS(output.SRFactoryCode, out coord) &&
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
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add("Lon", cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                    props.Add("Lat", cdms.ToString("Y" + splits[1].Trim(), new CoordinateDMSFormatter()));
                                }
                                else
                                {
                                    props.Add("Lat", cdms.ToString("A0°B0'C0.0\"N", new CoordinateDMSFormatter()));
                                    props.Add("Lon", cdms.ToString("X0°Y0'Z0.0\"E", new CoordinateDMSFormatter()));
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.DDM:
                        CoordinateDDM ddm;
                        if (coordinateGetter.CanGetDDM(output.SRFactoryCode, out coord) &&
                            CoordinateDDM.TryParse(coord, out ddm))
                        {
                            output.OutputCoordinate = ddm.ToString(output.Format, new CoordinateDDMFormatter());
                            var splits = output.Format.Split(new char[] { 'X' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                props.Add("Lat", ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                props.Add("Lon", ddm.ToString("X" + splits[1].Trim(), new CoordinateDDMFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add("Lon", ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                    props.Add("Lat", ddm.ToString("Y" + splits[1].Trim(), new CoordinateDDMFormatter()));
                                }
                                else
                                {
                                    props.Add("Lat", ddm.ToString("A0°B0.0#####'N", new CoordinateDDMFormatter()));
                                    props.Add("Lon", ddm.ToString("X0°Y0.0#####'E", new CoordinateDDMFormatter()));
                                }
                            }
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.GARS:
                        CoordinateGARS gars;
                        if (coordinateGetter.CanGetGARS(output.SRFactoryCode, out coord) &&
                            CoordinateGARS.TryParse(coord, out gars))
                        {
                            output.OutputCoordinate = gars.ToString(output.Format, new CoordinateGARSFormatter());
                            props.Add("Lon", gars.LonBand.ToString());
                            props.Add("Lat", gars.LatBand);
                            props.Add("Quadrant", gars.Quadrant.ToString());
                            props.Add("Key", gars.Key.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.MGRS:
                        CoordinateMGRS mgrs;
                        if (coordinateGetter.CanGetMGRS(output.SRFactoryCode, out coord) &&
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
                        CoordinateUSNG usng;
                        if (coordinateGetter.CanGetUSNG(output.SRFactoryCode, out coord) &&
                            CoordinateUSNG.TryParse(coord, out usng))
                        {
                            output.OutputCoordinate = usng.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add("GZD", usng.GZD);
                            props.Add("Grid Sq", usng.GS);
                            props.Add("Easting", usng.Easting.ToString());
                            props.Add("Northing", usng.Northing.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.UTM: 
                        CoordinateUTM utm;
                        if (coordinateGetter.CanGetUTM(output.SRFactoryCode, out coord) &&
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

        private void OnSelectSpatialReference(object obj)
        {
            if (coordinateGetter == null)
                return;

            coordinateGetter.SelectSpatialReference();
        }


    }
}
