/******************************************************************************* 
  * Copyright 2015 Esri 
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
using System.Collections.Generic;
using System.Linq;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.Helpers;

namespace CoordinateConversionLibrary.ViewModels
{
    public class CoordinateConversionViewModel : BaseViewModel
    {
        public CoordinateConversionViewModel()
        {
            OCView = new OutputCoordinateView();

            // set default CoordinateGetter
            coordinateGetter = new CoordinateGetBase();

            Mediator.Register(CoordinateConversionLibrary.Constants.RequestOutputUpdate, OnUpdateOutputs);
            Mediator.Register(CoordinateConversionLibrary.Constants.SelectSpatialReference, OnSelectSpatialReference);
            Mediator.Register(CoordinateConversionLibrary.Constants.SetCoordinateGetter, OnSetCoordinateGetter);
        }


        public OutputCoordinateView OCView { get; set; }
        private CoordinateGetBase coordinateGetter;

        public static CoordinateConversionLibraryConfig AddInConfig = new CoordinateConversionLibraryConfig();

        // InputCoordinate
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
                coordinateGetter.InputCoordinate = value;
                UpdateOutputs();
            }
        }

        private void OnSetCoordinateGetter(object obj)
        {
            coordinateGetter = obj as CoordinateGetBase;
        }

        private void OnUpdateOutputs(object obj)
        {
            UpdateOutputs();
        }

        private void UpdateOutputs()
        {
            foreach( var output in AddInConfig.OutputCoordinateList)
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
                                props.Add(Properties.Resources.StringLat, cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                props.Add(Properties.Resources.StringLon, cdd.ToString("X" + splits[1].Trim(), new CoordinateDDFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, cdd.ToString(splits[0].Trim(), new CoordinateDDFormatter()));
                                    props.Add(Properties.Resources.StringLat, cdd.ToString("Y" + splits[1].Trim(), new CoordinateDDFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, cdd.Lat.ToString());
                                    props.Add(Properties.Resources.StringLon, cdd.Lon.ToString());
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
                                props.Add(Properties.Resources.StringLat, cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                props.Add(Properties.Resources.StringLon, cdms.ToString("X" + splits[1].Trim(), new CoordinateDMSFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, cdms.ToString(splits[0].Trim(), new CoordinateDMSFormatter()));
                                    props.Add(Properties.Resources.StringLat, cdms.ToString("Y" + splits[1].Trim(), new CoordinateDMSFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, cdms.ToString("A0°B0'C0.0\"N", new CoordinateDMSFormatter()));
                                    props.Add(Properties.Resources.StringLon, cdms.ToString("X0°Y0'Z0.0\"E", new CoordinateDMSFormatter()));
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
                                props.Add(Properties.Resources.StringLat, ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                props.Add(Properties.Resources.StringLon, ddm.ToString("X" + splits[1].Trim(), new CoordinateDDMFormatter()));
                            }
                            else
                            {
                                splits = output.Format.Split(new char[] { 'Y' }, StringSplitOptions.RemoveEmptyEntries);
                                if (splits.Count() == 2)
                                {
                                    props.Add(Properties.Resources.StringLon, ddm.ToString(splits[0].Trim(), new CoordinateDDMFormatter()));
                                    props.Add(Properties.Resources.StringLat, ddm.ToString("Y" + splits[1].Trim(), new CoordinateDDMFormatter()));
                                }
                                else
                                {
                                    props.Add(Properties.Resources.StringLat, ddm.ToString("A0°B0.0#####'N", new CoordinateDDMFormatter()));
                                    props.Add(Properties.Resources.StringLon, ddm.ToString("X0°Y0.0#####'E", new CoordinateDDMFormatter()));
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
                            props.Add(Properties.Resources.StringLon, gars.LonBand.ToString());
                            props.Add(Properties.Resources.StringLat, gars.LatBand);
                            props.Add(Properties.Resources.StringQuadrant, gars.Quadrant.ToString());
                            props.Add(Properties.Resources.StringKey, gars.Key.ToString());
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.MGRS:
                        CoordinateMGRS mgrs;
                        if (coordinateGetter.CanGetMGRS(output.SRFactoryCode, out coord) &&
                            CoordinateMGRS.TryParse(coord, out mgrs))
                        {
                            output.OutputCoordinate = mgrs.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add(Properties.Resources.StringGZD, mgrs.GZD);
                            props.Add(Properties.Resources.StringGridSq, mgrs.GS);
                            props.Add(Properties.Resources.StringEasting, mgrs.Easting.ToString("00000"));
                            props.Add(Properties.Resources.StringNorthing, mgrs.Northing.ToString("00000"));
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.USNG:
                        CoordinateUSNG usng;
                        if (coordinateGetter.CanGetUSNG(output.SRFactoryCode, out coord) &&
                            CoordinateUSNG.TryParse(coord, out usng))
                        {
                            output.OutputCoordinate = usng.ToString(output.Format, new CoordinateMGRSFormatter());
                            props.Add(Properties.Resources.StringGZD, usng.GZD);
                            props.Add(Properties.Resources.StringGridSq, usng.GS);
                            props.Add(Properties.Resources.StringEasting, usng.Easting.ToString("00000"));
                            props.Add(Properties.Resources.StringNorthing, usng.Northing.ToString("00000"));
                            output.Props = props;
                        }
                        break;
                    case CoordinateType.UTM: 
                        CoordinateUTM utm;
                        if (coordinateGetter.CanGetUTM(output.SRFactoryCode, out coord) &&
                            CoordinateUTM.TryParse(coord, out utm))
                        {
                            output.OutputCoordinate = utm.ToString(output.Format, new CoordinateUTMFormatter());
                            props.Add(Properties.Resources.StringZone, utm.Zone.ToString() + utm.Hemi);
                            props.Add(Properties.Resources.StringEasting, utm.Easting.ToString("000000"));
                            props.Add(Properties.Resources.StringNorthing, utm.Northing.ToString("0000000"));
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
