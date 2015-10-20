using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CoordinateToolLibrary.Views;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.Helpers;
using ArcGIS.Core.Geometry;
using CoordinateToolLibrary.ViewModels;

namespace ProAppCoordToolModule
{
    internal class CoordinateToolDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ProAppCoordToolModule_CoordinateToolDockpane";

        protected CoordinateToolDockpaneViewModel() 
        {
            _coordinateToolView = new CoordinateToolView();
            HasInputError = false;
            AddNewOCCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnAddNewOCCommand);
        }

        private ProCoordinateGet proCoordGetter = new ProCoordinateGet();

        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                //TODO RaisePropertyChanged(() => HasInputError);
            }
        }

        public CoordinateToolLibrary.Helpers.RelayCommand AddNewOCCommand { get; set; }

        private string _inputCoordinate;
        public string InputCoordinate
        {
            get
            {
                return _inputCoordinate;
            }

            set
            {
                _inputCoordinate = value;
                var tempDD = ProcessInput(_inputCoordinate);

                // update tool view model
                var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
                if (ctvm != null)
                {
                    ctvm.SetCoordinateGetter(proCoordGetter);
                    ctvm.InputCoordinate = tempDD;
                }
            }
        }

        private CoordinateToolView _coordinateToolView;
        public CoordinateToolView CTView
        {
            get
            {
                return _coordinateToolView;
            }
            set
            {
                _coordinateToolView = value;
            }
        }

        private void OnAddNewOCCommand(object obj)
        {
            // Get name from user
            string name = "Temp";
            Mediator.NotifyColleagues("AddNewOutputCoordinate", new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#N X0.0#E" });
        }

        private string ProcessInput(string input)
        {
            string result = string.Empty;
            //ESRI.ArcGIS.Geometry.IPoint point;
            MapPoint point;
            //HasInputError = false;

            //if (string.IsNullOrWhiteSpace(input))
            //    return result;

            //var coordType = GetCoordinateType(input, out point);

            //if (coordType == CoordinateType.Unknown)
            //    HasInputError = true;
            //else
            //{
            //    proCoordGetter.Point = point;
            //    result = (point as IConversionNotation).GetDDFromCoords(6);
            //}

            return result;
        }

        //private CoordinateType GetCoordinateType(string input, out ESRI.ArcGIS.Geometry.IPoint point)
        //{
        //    point = new PointClass();
        //    var cn = point as IConversionNotation;
        //    Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
        //    System.Object obj = Activator.CreateInstance(t);
        //    ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

        //    // Use the enumeration to create an instance of the predefined object.

        //    IGeographicCoordinateSystem geographicCS =
        //        srFact.CreateGeographicCoordinateSystem((int)
        //        esriSRGeoCSType.esriSRGeoCS_WGS1984);

        //    point.SpatialReference = geographicCS;

        //    try
        //    {
        //        cn.PutCoordsFromDD(input);
        //        return CoordinateType.DD;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromDDM(input);
        //        return CoordinateType.DDM;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromDMS(input);
        //        return CoordinateType.DMS;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, input);
        //        return CoordinateType.GARS;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeLL, input);
        //        return CoordinateType.GARS;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromUSNG(input);
        //        return CoordinateType.USNG;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMAddSpaces, input);
        //        return CoordinateType.UTM;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, input);
        //        return CoordinateType.UTM;
        //    }
        //    catch { }

        //    try
        //    {
        //        cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMUseNS, input);
        //        return CoordinateType.UTM;
        //    }
        //    catch { }

        //    return CoordinateType.Unknown;
        //}

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Coordinate Notation Tool";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CoordinateToolDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CoordinateToolDockpaneViewModel.Show();
        }
    }
}
