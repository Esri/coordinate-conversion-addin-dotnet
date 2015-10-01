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

        public bool HasInputError { get; set; }

        public string DD { get; set; }
        public string DDM { get; set; }
        public string DMS { get; set; }
        public string GARS { get; set; }
        public string MGRS { get; set; }
        public string USNG { get; set; }
        public string UTM { get; set; }

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

                // do this or use a mediator
                //var ctvm = CTView.DataContext as CoordinateToolViewModel;
                //if (ctvm != null)
                //    ctvm.InputCoordinate = tempDD;
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


            var coordType = GetCoordinateType(input, out point);

            if (coordType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                UpdateOutputs(point);
            }

            RaisePropertyChanged(() => HasInputError);
            
            return result;
        }

        private void UpdateOutputs(IPoint point)
        {
            var cn = point as IConversionNotation;

            DD = cn.GetDDFromCoords(4);
            DDM = cn.GetDDMFromCoords(4);
            DMS = cn.GetDMSFromCoords(4);
            GARS = cn.GetGARSFromCoords();
            MGRS = (point as IConversionMGRS).CreateMGRS(4, true, esriMGRSModeEnum.esriMGRSMode_Automatic);
            USNG = cn.GetUSNGFromCoords(4, true, true);
            UTM = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMUseNS);

            RaisePropertyChanged(() => DD); 
            RaisePropertyChanged(() => DDM);
            RaisePropertyChanged(() => DMS);
            RaisePropertyChanged(() => GARS);
            RaisePropertyChanged(() => MGRS);
            RaisePropertyChanged(() => USNG);
            RaisePropertyChanged(() => UTM);
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
