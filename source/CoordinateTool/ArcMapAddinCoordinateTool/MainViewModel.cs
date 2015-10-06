using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Views;
using CoordinateToolLibrary.ViewModels;
using ESRI.ArcGIS.Geometry;
using System.Windows;
using CoordinateToolLibrary.Models;

namespace ArcMapAddinCoordinateTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            _coordinateToolView = new CoordinateToolView();
            HasInputError = false;
        }

        private ArcMapCoordinateGet amCoordGetter = new ArcMapCoordinateGet();

        public bool HasInputError { get; set; }

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
                    ctvm.SetCoordinateGetter(amCoordGetter);
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

        private string ProcessInput(string input)
        {
            string result = string.Empty;
            ESRI.ArcGIS.Geometry.IPoint point;

            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var coordType = GetCoordinateType(input, out point);

            if (coordType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                amCoordGetter.Point = point;
                result = (point as IConversionNotation).GetDDFromCoords(6);
            }

            RaisePropertyChanged(() => HasInputError);
            
            return result;
        }

        private CoordinateType GetCoordinateType(string input, out ESRI.ArcGIS.Geometry.IPoint point)
        {
            point = new PointClass();
            var cn = point as IConversionNotation;
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            point.SpatialReference = geographicCS;

            try
            {
                cn.PutCoordsFromDD(input);
                return CoordinateType.DD;
            }
            catch { }

            try
            {
                cn.PutCoordsFromDDM(input);
                return CoordinateType.DDM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromDMS(input);
                return CoordinateType.DMS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, input);
                return CoordinateType.GARS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeLL, input);
                return CoordinateType.GARS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
                return CoordinateType.MGRS;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUSNG(input);
                return CoordinateType.USNG;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMAddSpaces, input);
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, input);
                return CoordinateType.UTM;
            }
            catch { }

            try
            {
                cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMUseNS, input);
                return CoordinateType.UTM;
            }
            catch { }

            return CoordinateType.Unknown;
        }
    }
}
