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
using System.ComponentModel;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;

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
            ActivatePointToolCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnMapToolCommand);
            FlashPointCommand = new CoordinateToolLibrary.Helpers.RelayCommand(OnFlashPointCommand);
            //TODO register broadcast coordinate needed
        }
        private static System.IDisposable _overlayObject = null;
        private async void OnFlashPointCommand(object obj)
        {
            CoordinateDD dd;
            var ctvm = CTView.Resources["CTViewModel"] as CoordinateToolViewModel;
            if (ctvm != null)
            {
                if (!CoordinateDD.TryParse(ctvm.InputCoordinate, out dd))
                    return;
            }
            else { return; }

            ArcGIS.Core.CIM.CIMPointSymbol symbol = null;
            var point = await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
            });

            await QueuedTask.Run(() =>
            {
                // Construct point symbol
                symbol = SymbolFactory.ConstructPointSymbol(ColorFactory.Red, 10.0, SimpleMarkerStyle.Star);
            });

            //Get symbol reference from the symbol 
            CIMSymbolReference symbolReference = symbol.MakeSymbolReference();

            await QueuedTask.Run(() =>
            {
                if(_overlayObject != null)
                {
                    _overlayObject.Dispose();
                    _overlayObject = null;
                }
                _overlayObject = MapView.Active.AddOverlay(point, symbolReference);
                MapView.Active.ZoomToAsync(point, new TimeSpan(2500000), true);
            });
        }

        private void OnMapToolCommand(object obj)
        {
            FrameworkApplication.SetCurrentToolAsync("ProAppCoordToolModule_CoordinateMapTool");
        }

        private ProCoordinateGet proCoordGetter = new ProCoordinateGet();

        private bool _hasInputError = false;
        public bool HasInputError
        {
            get { return _hasInputError; }
            set
            {
                _hasInputError = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("HasInputError"));
            }
        }

        public CoordinateToolLibrary.Helpers.RelayCommand AddNewOCCommand { get; set; }
        public CoordinateToolLibrary.Helpers.RelayCommand ActivatePointToolCommand { get; set; }
        public CoordinateToolLibrary.Helpers.RelayCommand FlashPointCommand { get; set; }

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

                NotifyPropertyChanged(new PropertyChangedEventArgs("InputCoordinate"));
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
            HasInputError = false;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var coordType = GetCoordinateType(input, out point);

            if (coordType == CoordinateType.Unknown)
                HasInputError = true;
            else
            {
                proCoordGetter.Point = point;
                result = new CoordinateDD(point.Y, point.X).ToString("", new CoordinateDDFormatter());
            }

            return result;
        }

        private CoordinateType GetCoordinateType(string input, out MapPoint point)
        {
            point = null;

            // DD
            CoordinateDD dd;
            if(CoordinateDD.TryParse(input, out dd))
            {
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DD;
            }

            // DDM
            CoordinateDDM ddm;
            if(CoordinateDDM.TryParse(input, out ddm))
            {
                dd = new CoordinateDD(ddm);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DDM;
            }
            // DMS
            CoordinateDMS dms;
            if (CoordinateDMS.TryParse(input, out dms))
            {
                dd = new CoordinateDD(dms);
                point = QueuedTask.Run(() =>
                {
                    ArcGIS.Core.Geometry.SpatialReference sptlRef = SpatialReferenceBuilder.CreateSpatialReference(4326);
                    return MapPointBuilder.CreateMapPoint(dd.Lon, dd.Lat, sptlRef);
                }).Result;
                return CoordinateType.DMS;
            }

            return CoordinateType.Unknown;
        }

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
