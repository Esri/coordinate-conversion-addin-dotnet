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
using System.Linq;
using System.Windows.Data;
using System.Windows.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            //TODO this is crashing ArcMap on load
            ConvertTabView = new CCConvertTabView();
            ConvertTabView.DataContext = new ConvertTabViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new CollectTabViewModel();

            #region old code
            //HasInputError = false;
            //IsHistoryUpdate = true;
            //IsToolGenerated = false;
            //ListBoxItemAddInPoint = null;

            //AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            //ActivatePointToolCommand = new RelayCommand(OnActivatePointToolCommand);
            //EditPropertiesDialogCommand = new RelayCommand(OnEditPropertiesDialogCommand);
            //FlashPointCommand = new RelayCommand(OnFlashPointCommand);
            //CopyAllCommand = new RelayCommand(OnCopyAllCommand);
            //DeletePointCommand = new RelayCommand(OnDeletePointCommand);
            //DeleteAllPointsCommand = new RelayCommand(OnDeleteAllPointsCommand);
            //ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);

            //Mediator.Register(CoordinateConversionLibrary.Constants.RequestCoordinateBroadcast, OnBCNeeded);
            //Mediator.Register(CoordinateConversionLibrary.Constants.SetListBoxItemAddInPoint, OnSetListBoxItemAddInPoint);
            //InputCoordinateHistoryList = new ObservableCollection<string>();
            //CoordinateAddInPoints = new ObservableCollection<AddInPoint>();

            //ToolMode = MapPointToolMode.Unknown;

            // update tool view model
            //TODO this need to be a Mediator call
            //var ctvm = ConvertTabView.Resources["CTViewModel"] as CoordinateConversionViewModel;
            //if (ctvm != null)
            //{
            //    ctvm.SetCoordinateGetter(amCoordGetter);
            //}

            //configObserver = new PropertyObserver<CoordinateConversionLibraryConfig>(CoordinateConversionViewModel.AddInConfig)
            //.RegisterHandler(n => n.DisplayCoordinateType, n => 
            //{
            //    if (amCoordGetter != null && amCoordGetter.Point != null)
            //    {
            //        InputCoordinate = amCoordGetter.GetInputDisplayString();
            //    }
            //});
            #endregion
        }

        public CCConvertTabView ConvertTabView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        object selectedTab = null;
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value)
                    return;

                selectedTab = value;
                var tabItem = selectedTab as TabItem;
                Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
                //TODO let the other viewmodels determine what to do when tab selection changes
                if (tabItem.Header.ToString() == CoordinateConversionLibrary.Properties.Resources.HeaderCollect)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Collect);
                else
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Convert);
            }
        }

        #region older code
        //public AddInPoint ListBoxItemAddInPoint { get; set; }

        //private void OnSetListBoxItemAddInPoint(object obj)
        //{
        //    ListBoxItemAddInPoint = obj as AddInPoint;
        //}

        //PropertyObserver<CoordinateConversionLibraryConfig> configObserver;

        //public bool IsToolActive
        //{
        //    get
        //    {
        //        if(ArcMap.Application.CurrentTool != null)
        //            return ArcMap.Application.CurrentTool.Name == "ESRI_ArcMapAddinCoordinateConversion_PointTool";

        //        return false;
        //    }
        //    set
        //    {
        //        if (value)
        //            OnActivatePointToolCommand(null);
        //        else
        //            if (ArcMap.Application.CurrentTool != null)
        //                ArcMap.Application.CurrentTool = null;

        //        RaisePropertyChanged(() => IsToolActive);
        //    }
        //}

        //public CoordinateType InputCoordinateType { get; set; }
        //public ObservableCollection<string> InputCoordinateHistoryList { get; set; }
        //public ObservableCollection<AddInPoint> CoordinateAddInPoints { get; set; }


        //public MapPointToolMode ToolMode { get; set; }

        //// lists to store GUIDs of graphics, temp feedback and map graphics
        //private static List<AMGraphic> GraphicsList = new List<AMGraphic>();

        //private void OnCopyAllCommand(object obj)
        //{
        //    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.CopyAllCoordinateOutputs, InputCoordinate);
        //}
        //private ISpatialReference GetSR(int type)
        //{
        //    Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
        //    System.Object obj = Activator.CreateInstance(t);
        //    ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

        //    // Use the enumeration to create an instance of the predefined object.
        //    try
        //    {
        //        IGeographicCoordinateSystem geographicCS = srFact.CreateGeographicCoordinateSystem(type);

        //        return geographicCS as ISpatialReference;
        //    }
        //    catch ( Exception ex)
        //    {
        //        // do nothing
        //    }


        //    try
        //    {
        //        IProjectedCoordinateSystem projectCS = srFact.CreateProjectedCoordinateSystem(type);

        //        return projectCS as ISpatialReference;
        //    }
        //    catch(Exception ex)
        //    {
        //        // do nothing
        //    }

        //    return null;
        //}
        //private ISpatialReference GetSR()
        //{
        //    Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
        //    System.Object obj = Activator.CreateInstance(t);
        //    ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

        //    // Use the enumeration to create an instance of the predefined object.

        //    IGeographicCoordinateSystem geographicCS =
        //        srFact.CreateGeographicCoordinateSystem((int)
        //        esriSRGeoCSType.esriSRGeoCS_WGS1984);

        //    return geographicCS as ISpatialReference;
        //}
        //private void OnFlashPointCommand(object obj)
        //{
        //    IGeometry address = null;

        //    if(amCoordGetter != null && amCoordGetter.Point != null)
        //        address = amCoordGetter.Point;

        //    if (ListBoxItemAddInPoint != null)
        //    {
        //        address = ListBoxItemAddInPoint.Point;
        //        ListBoxItemAddInPoint = null;
        //    }

        //    if(address != null)
        //    {
        //        // Map und View
        //        IMxDocument mxdoc = ArcMap.Application.Document as IMxDocument;
        //        IActiveView activeView = mxdoc.ActivatedView;
        //        IMap map = mxdoc.FocusMap;
        //        IEnvelope envelope = activeView.Extent;

        //        //ClearGraphicsContainer(map);

        //        IScreenDisplay screenDisplay = activeView.ScreenDisplay;
        //        short screenCache = Convert.ToInt16(esriScreenCache.esriNoScreenCache);

        //        ISpatialReference outgoingCoordSystem = map.SpatialReference;
        //        address.Project(outgoingCoordSystem);

        //        // is point within current extent
        //        // if so, pan to point
        //        var relationOp = envelope as IRelationalOperator;
        //        if (relationOp != null && activeView is IMap)
        //        {
        //            if (!relationOp.Contains(address))
        //            {
        //                // pan to
        //                envelope.CenterAt(address as IPoint);
        //                activeView.Extent = envelope;
        //                activeView.Refresh();
        //            }
        //        }

        //        IRgbColor color = new RgbColorClass();
        //        color.Green = 80;
        //        color.Red = 22;
        //        color.Blue = 68;

        //        ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
        //        simpleMarkerSymbol.Color = color;
        //        simpleMarkerSymbol.Size = 15;
        //        simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

        //        IElement element = null;

        //        IMarkerElement markerElement = new MarkerElementClass();

        //        markerElement.Symbol = simpleMarkerSymbol;
        //        element = markerElement as IElement;

        //        IPolygon poly = null;
        //        if (InputCoordinateType == CoordinateType.MGRS || InputCoordinateType == CoordinateType.USNG)
        //        {
        //            poly = GetMGRSPolygon(address as IPoint);
        //        }

        //        if (poly != null)
        //        {
        //            address = poly;
        //        }
        //        var av = mxdoc.FocusMap as IActiveView;
        //        FlashGeometry(address, color, av.ScreenDisplay, 500, av.Extent);

        //        //AddElement(map, address);

        //        // do not center if in layout view
        //        //if (mxdoc.ActiveView is IMap)
        //        //{
        //        //    if (poly != null && !poly.IsEmpty && (poly as IArea) != null)
        //        //        envelope.CenterAt((poly as IArea).Centroid);
        //        //    else
        //        //        envelope.CenterAt(amCoordGetter.Point);

        //        //    activeView.Extent = envelope;
        //        //    activeView.Refresh();
        //        //}
        //    }
        //}

        //private IPolygon GetMGRSPolygon(IPoint point)
        //{
        //    CoordinateMGRS mgrs;

        //    IPointCollection pc = new RingClass();

        //    // bottom left
        //    CoordinateMGRS.TryParse(InputCoordinate, out mgrs);

        //    // don't create a polygon for 1m resolution
        //    if (mgrs.Easting.ToString().Length > 4 && mgrs.Northing.ToString().Length > 4)
        //        return null;

        //    var tempPoint = new PointClass() as IConversionNotation;
        //    (tempPoint as IPoint).SpatialReference = GetSR();
        //    var anotherMGRSstring = mgrs.ToString("", new CoordinateMGRSFormatter());
        //    tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
        //    pc.AddPoint(tempPoint as IPoint);
            
        //    // top left
        //    var tempMGRS = new CoordinateMGRS(mgrs.GZD, mgrs.GS, mgrs.Easting, mgrs.Northing);
        //    var tempEasting = mgrs.Easting.ToString().PadRight(5,'0');
        //    tempMGRS.Easting = Convert.ToInt32(tempEasting);
        //    var tempNorthing = mgrs.Northing.ToString().PadRight(5,'9');
        //    tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0','9'));

        //    tempPoint = new PointClass() as IConversionNotation;
        //    (tempPoint as IPoint).SpatialReference = GetSR();
        //    anotherMGRSstring = tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
        //    tempPoint.PutCoordsFromMGRS(anotherMGRSstring, esriMGRSModeEnum.esriMGRSMode_Automatic);
        //    pc.AddPoint(tempPoint as IPoint);

        //    // top right
        //    tempEasting = mgrs.Easting.ToString().PadRight(5,'9');
        //    tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
        //    tempNorthing = mgrs.Northing.ToString().PadRight(5,'9');
        //    tempMGRS.Northing = Convert.ToInt32(tempNorthing.Replace('0', '9'));

        //    tempPoint = new PointClass() as IConversionNotation;
        //    (tempPoint as IPoint).SpatialReference = GetSR();
        //    tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
        //    pc.AddPoint(tempPoint as IPoint);

        //    // bottom right
        //    tempEasting = mgrs.Easting.ToString().PadRight(5,'9');
        //    tempMGRS.Easting = Convert.ToInt32(tempEasting.Replace('0', '9'));
        //    tempNorthing = mgrs.Northing.ToString().PadRight(5,'0');
        //    tempMGRS.Northing = Convert.ToInt32(tempNorthing);

        //    tempPoint = new PointClass() as IConversionNotation;
        //    (tempPoint as IPoint).SpatialReference = GetSR();
        //    tempPoint.PutCoordsFromMGRS(tempMGRS.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_Automatic);
        //    pc.AddPoint(tempPoint as IPoint);

        //    // create polygon
        //    var poly = new PolygonClass();
        //    poly.SpatialReference = GetSR();
        //    poly.AddPointCollection(pc);
        //    poly.Close();

        //    return poly;
        //}
        //private void AddElement(IMap map, IGeometry geom)
        //{
        //    IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
        //    IRgbColor color = new RgbColorClass();
        //    color.Green = 80;
        //    color.Red = 22;
        //    color.Blue = 68;

        //    IElement element = null;

        //    if (geom is IPoint)
        //    {
        //        ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
        //        simpleMarkerSymbol.Color = color;
        //        simpleMarkerSymbol.Size = 15;
        //        simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

        //        IMarkerElement markerElement = new MarkerElementClass();

        //        markerElement.Symbol = simpleMarkerSymbol;
        //        element = markerElement as IElement;

        //        if (element != null)
        //        {
        //            element.Geometry = geom;
        //        }
        //    }
        //    else if(geom is IPolygon)
        //    {
        //        var temp = new SimpleLineSymbol();
        //        temp.Color = color;
        //        temp.Style = esriSimpleLineStyle.esriSLSSolid;
        //        temp.Width = 2;
        //        var s = new SimpleFillSymbol();
        //        s.Color = color;
        //        s.Outline = temp;
        //        s.Style = esriSimpleFillStyle.esriSFSBackwardDiagonal;

        //        var pe = new PolygonElementClass();
        //        element = pe as IElement;
        //        var fill = pe as IFillShapeElement;
        //        fill.Symbol = s;
        //        element.Geometry = geom;
        //    }
        //    graphicsContainer.AddElement(element, 0);
        //    IActiveView activeView = map as IActiveView;
        //    activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        //}
        //private void ClearGraphicsContainer(IMap map)
        //{
        //    var graphicsContainer = map as IGraphicsContainer;
        //    if (graphicsContainer != null)
        //    {
        //        //graphicsContainer.DeleteAllElements();
        //        // now we have a collection feature and need to not clear those related graphics
        //        graphicsContainer.Reset();
        //        var g = graphicsContainer.Next();
        //        while(g != null)
        //        {
        //            if(!CoordinateAddInPoints.Any(aiPoint => aiPoint.GUID == ((IElementProperties)g).Name))
        //                graphicsContainer.DeleteElement(g);
        //            g = graphicsContainer.Next();
        //        }
        //    }
        //}
        //private void AddElement(IMap map, IPoint point)
        //{
        //    IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
        //    IRgbColor color = new RgbColorClass();
        //    color.Green = 80;
        //    color.Red = 22;
        //    color.Blue = 68;

        //    ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
        //    simpleMarkerSymbol.Color = color;
        //    simpleMarkerSymbol.Size = 15;
        //    simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;

        //    IElement element = null;

        //    IMarkerElement markerElement = new MarkerElementClass();

        //    markerElement.Symbol = simpleMarkerSymbol;
        //    element = markerElement as IElement;

        //    if (element != null)
        //    {
        //        element.Geometry = point;
        //    }
        //    graphicsContainer.AddElement(element, 0);

        //    //Flag the new text to invalidate.
        //    IActiveView activeView = map as IActiveView;
        //    activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        //}
        //private void AddTextElement(IMap map, IPoint point, string text)
        //{
        //    IGraphicsContainer graphicsContainer = map as IGraphicsContainer;
        //    IElement element = new TextElementClass();
        //    ITextElement textElement = element as ITextElement;

        //    element.Geometry = point;
        //    textElement.Text = text;
        //    graphicsContainer.AddElement(element, 0);

        //    //Flag the new text to invalidate.
        //    IActiveView activeView = map as IActiveView;
        //    activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        //}
        //private void OnActivatePointToolCommand(object obj)
        //{
        //    SetToolActiveInToolBar(ArcMap.Application, "ESRI_ArcMapAddinCoordinateConversion_PointTool");
        //}

        //public void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
        //{
        //    ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
        //    ESRI.ArcGIS.esriSystem.UID commandID = new ESRI.ArcGIS.esriSystem.UIDClass();
        //    commandID.Value = toolName; // example: "esriArcMapUI.ZoomInTool";
        //    ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(commandID, false, false);

        //    if (commandItem != null)
        //        application.CurrentTool = commandItem;
        //}
        
        //private void OnEditPropertiesDialogCommand(object obj)
        //{
        //    var dlg = new EditPropertiesView();

        //    dlg.ShowDialog();
        //}

        //private void OnAddNewOCCommand(object obj)
        //{
        //    // Get name from user
        //    string name = CoordinateType.DD.ToString();
        //    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.AddNewOutputCoordinate, new OutputCoordinateModel() { Name = name, CType = CoordinateType.DD, Format = "Y0.0#N X0.0#E" });
        //}

        //private void OnDeletePointCommand(object obj)
        //{
        //    var items = obj as IList;
        //    var objects = items.Cast<AddInPoint>().ToList();

        //    if (objects == null)
        //        return;

        //    DeletePoints(objects);
        //}

        //private void OnDeleteAllPointsCommand(object obj)
        //{
        //    DeletePoints(CoordinateAddInPoints.ToList());
        //}

        //private void OnClearGraphicsCommand(object obj)
        //{
        //    var mxdoc = ArcMap.Application.Document as IMxDocument;
        //    if (mxdoc == null)
        //        return;
        //    var av = mxdoc.FocusMap as IActiveView;
        //    if (av == null)
        //        return;
        //    var gc = av as IGraphicsContainer;
        //    if (gc == null)
        //        return;
        //    //TODO need to clarify what clear graphics button does
        //    // seems to be different than the other Military Tools when doing batch collection
        //    RemoveGraphics(gc, GraphicsList.Where(g => g.IsTemp == false).ToList());

        //    //av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        //    av.Refresh(); // sometimes a partial refresh is not working
        //}

        //private ArcMapCoordinateGet amCoordGetter = new ArcMapCoordinateGet();

        //private bool _hasInputError = false;
        //public bool HasInputError 
        //{
        //    get { return _hasInputError; }
        //    set
        //    {
        //        _hasInputError = value;
        //        RaisePropertyChanged(() => HasInputError);
        //    }
        //}

        //public RelayCommand AddNewOCCommand { get; set; }
        //public RelayCommand ActivatePointToolCommand { get; set; }
        //public RelayCommand EditPropertiesDialogCommand { get; set; }
        //public RelayCommand FlashPointCommand { get; set; }
        //public RelayCommand CopyAllCommand { get; set; }
        //public RelayCommand DeletePointCommand { get; set; }
        //public RelayCommand DeleteAllPointsCommand { get; set; }
        //public RelayCommand ClearGraphicsCommand { get; set; }

        //public bool IsHistoryUpdate { get; set; }
        //public bool IsToolGenerated { get; set; }

        //private string _inputCoordinate;
        //public string InputCoordinate
        //{
        //    get
        //    {
        //        return _inputCoordinate;
        //    }

        //    set
        //    {
        //        if (string.IsNullOrWhiteSpace(value))
        //            return;

        //        _inputCoordinate = value;
        //        var tempDD = ProcessInput(_inputCoordinate);

        //        // update tool view model
        //        var ctvm = ConvertTabView.Resources["CTViewModel"] as CoordinateConversionViewModel;
        //        if (ctvm != null)
        //        {
        //            ctvm.SetCoordinateGetter(amCoordGetter);
        //            ctvm.InputCoordinate = tempDD;
        //            var formattedInputCoordinate = amCoordGetter.GetInputDisplayString();
        //            // update history
        //            if (IsHistoryUpdate)
        //            {
        //                if (IsToolGenerated)
        //                {
        //                    UIHelpers.UpdateHistory(formattedInputCoordinate, InputCoordinateHistoryList);
        //                }
        //                else
        //                {
        //                    UIHelpers.UpdateHistory(_inputCoordinate, InputCoordinateHistoryList);
        //                }
        //            }
        //            // reset flags
        //            IsHistoryUpdate = true;
        //            IsToolGenerated = false;

        //            _inputCoordinate = formattedInputCoordinate;
        //        }

        //        RaisePropertyChanged(() => InputCoordinate);
        //    }
        //}

        //private CCConvertTabView _coordinateConversionView;
        
        //{
        //    get
        //    {
        //        return _coordinateConversionView;
        //    }
        //    set
        //    {
        //        _coordinateConversionView = value;
        //    }
        //}

        //private object _ListBoxSelectedItem = null;
        //public object ListBoxSelectedItem
        //{
        //    get { return _ListBoxSelectedItem; }
        //    set
        //    {
        //        // we are using this to know when the selection changes
        //        // setting null will allow this setter to be called in multiple selection mode
        //        _ListBoxSelectedItem = null;
        //        RaisePropertyChanged(() => ListBoxSelectedItem);

        //        // update selections
        //        UpdateHighlightedGraphics();
        //    }
        //}

        //private int _ListBoxSelectedIndex = -1;
        //public int ListBoxSelectedIndex
        //{
        //    get { return _ListBoxSelectedIndex; }
        //    set
        //    {
        //        // this works with the ListBoxSelectedItem 
        //        // without this the un-selecting of 1 will not trigger an update
        //        _ListBoxSelectedIndex = value;
        //        RaisePropertyChanged(() => ListBoxSelectedIndex);
        //        UpdateHighlightedGraphics();
        //    }
        //}

        //private void UpdateHighlightedGraphics()
        //{
        //    var mxdoc = ArcMap.Application.Document as IMxDocument;
        //    var av = mxdoc.FocusMap as IActiveView;
        //    var gc = av as IGraphicsContainer;

        //    gc.Reset();
        //    var element = gc.Next();

        //    while(element != null)
        //    {
        //        var eProp = element as IElementProperties;

        //        if(eProp != null)
        //        {
        //            var aiPoint = CoordinateAddInPoints.FirstOrDefault(p => p.GUID == eProp.Name);

        //            if(aiPoint != null)
        //            {
        //                    // highlight
        //                var markerElement = element as IMarkerElement;
        //                if(markerElement != null)
        //                {
        //                    var sms = markerElement.Symbol as ISimpleMarkerSymbol;
        //                    if(sms != null)
        //                    {
        //                        var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;

        //                        simpleMarkerSymbol.Color = sms.Color;
        //                        simpleMarkerSymbol.Size = sms.Size;
        //                        simpleMarkerSymbol.Style = sms.Style;
        //                        simpleMarkerSymbol.OutlineSize = 1;

        //                        if (aiPoint.IsSelected)
        //                        {
        //                            var color = new RgbColorClass() { Green = 255 } as IColor;
        //                            // Marker symbols
        //                            simpleMarkerSymbol.Outline = true;
        //                            simpleMarkerSymbol.OutlineColor = color;
        //                        }
        //                        else
        //                        {
        //                            simpleMarkerSymbol.Outline = false;
        //                            //simpleMarkerSymbol.OutlineColor = sms.Color;
        //                        }

        //                        markerElement.Symbol = simpleMarkerSymbol;
                                
        //                        gc.UpdateElement(element);
        //                    }
        //                }
        //            }
        //        }


        //        element = gc.Next();
        //    }

        //    av.Refresh();
        //}


        //internal string GetFormattedCoordinate(string coord, CoordinateType cType)
        //{
        //    string format = "";

        //    var tt = CoordinateConversionViewModel.AddInConfig.OutputCoordinateList.FirstOrDefault(t => t.CType == cType);
        //    if (tt != null)
        //    {
        //        format = tt.Format;
        //        Console.WriteLine(tt.Format);
        //    }

        //    var cf = CoordinateHandler.GetFormattedCoord(cType, coord, format);

        //    if (!String.IsNullOrWhiteSpace(cf))
        //        return cf;

        //    return string.Empty;
        //}

        //private string ProcessInput(string input)
        //{
        //    string result = string.Empty;
        //    ESRI.ArcGIS.Geometry.IPoint point;

        //    HasInputError = false;

        //    if (string.IsNullOrWhiteSpace(input))
        //        return result;

        //    InputCoordinateType = GetCoordinateType(input, out point);

        //    if (InputCoordinateType == CoordinateType.Unknown)
        //        HasInputError = true;
        //    else
        //    {
        //        amCoordGetter.Point = point;
        //        try
        //        {
        //            if(CoordinateConversionViewModel.AddInConfig.DisplayCoordinateType == CoordinateConversionLibrary.CoordinateTypes.None)
        //            {
        //                result = string.Format("{0:0.0} {1:0.0}", point.Y, point.X);
        //            }
        //            else
        //                result = (point as IConversionNotation).GetDDFromCoords(6);
        //        }
        //        catch(Exception ex)
        //        {
        //            // do nothing
        //        }
        //    }

        //    return result;
        //}

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

        //    CoordinateGARS gars;
        //    if(CoordinateGARS.TryParse(input, out gars))
        //    {
        //        try
        //        {
        //            cn.PutCoordsFromGARS(esriGARSModeEnum.esriGARSModeCENTER, gars.ToString("", new CoordinateGARSFormatter()));
        //            return CoordinateType.GARS;
        //        }
        //        catch { }
        //    }

        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_Automatic);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }
        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewStyle);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }
        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_NewWith180InZone01);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }
        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldStyle);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }
        //    try
        //    {
        //        cn.PutCoordsFromMGRS(input, esriMGRSModeEnum.esriMGRSMode_OldWith180InZone01);
        //        return CoordinateType.MGRS;
        //    }
        //    catch { }

        //    // mgrs try parse
        //    CoordinateMGRS mgrs;
        //    if (CoordinateMGRS.TryParse(input, out mgrs))
        //    {
        //        try
        //        {
        //            cn.PutCoordsFromMGRS(mgrs.ToString("", new CoordinateMGRSFormatter()), esriMGRSModeEnum.esriMGRSMode_NewStyle);
        //            return CoordinateType.MGRS;
        //        }
        //        catch { }
        //    }

        //    try
        //    {
        //        cn.PutCoordsFromUSNG(input);
        //        return CoordinateType.USNG;
        //    }
        //    catch { }

        //    CoordinateUSNG usng;
        //    if(CoordinateUSNG.TryParse(input, out usng))
        //    {
        //        try
        //        {
        //            cn.PutCoordsFromUSNG(usng.ToString("", new CoordinateMGRSFormatter()));
        //            return CoordinateType.USNG;
        //        }
        //        catch { }
        //    }

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

        //    CoordinateUTM utm;
        //    if(CoordinateUTM.TryParse(input, out utm))
        //    {
        //        try
        //        {
        //            cn.PutCoordsFromUTM(esriUTMConversionOptionsEnum.esriUTMNoOptions, utm.ToString("", new CoordinateUTMFormatter()));
        //            return CoordinateType.UTM;
        //        }
        //        catch { }
        //    }

        //    Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

        //    var matchMercator = regexMercator.Match(input);

        //    if (matchMercator.Success && matchMercator.Length == input.Length)
        //    {
        //        try
        //        {
        //            var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
        //            var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
        //            point.X = Lon;
        //            point.Y = Lat;
        //            point.SpatialReference = GetSR((int)esriSRProjCS3Type.esriSRProjCS_WGS1984WebMercatorMajorAuxSphere);
        //            return CoordinateType.DD;
        //        }
        //        catch (Exception ex)
        //        {
        //            // do nothing
        //        }
        //    }

        //    return CoordinateType.Unknown;
        //}

        //private void OnBCNeeded(object obj)
        //{
        //    if(amCoordGetter == null || amCoordGetter.Point == null)
        //        return;

        //    BroadcastCoordinateValues(amCoordGetter.Point);
        //}

        //private void BroadcastCoordinateValues(ESRI.ArcGIS.Geometry.IPoint point)
        //{

        //    var dict = new Dictionary<CoordinateType, string>();
        //    if (point == null)
        //        return;

        //    var cn = point as IConversionNotation;

        //    if(cn == null)
        //        return;

        //    try
        //    {
        //        dict.Add(CoordinateType.DD, cn.GetDDFromCoords(6));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.DDM, cn.GetDDMFromCoords(6));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.DMS, cn.GetDMSFromCoords(6));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.GARS, cn.GetGARSFromCoords());
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.MGRS, cn.CreateMGRS(5, true, esriMGRSModeEnum.esriMGRSMode_Automatic));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.USNG, cn.GetUSNGFromCoords(5, true, false));
        //    }
        //    catch { }
        //    try
        //    {
        //        dict.Add(CoordinateType.UTM, cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS));
        //    }
        //    catch { }

        //    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.BroadcastCoordinateValues, dict);
        //}

        /////<summary>Flash geometry on the display. The geometry type could be polygon, polyline, point, or multipoint.</summary>
        /////
        /////<param name="geometry"> An IGeometry interface</param>
        /////<param name="color">An IRgbColor interface</param>
        /////<param name="display">An IDisplay interface</param>
        /////<param name="delay">A System.Int32 that is the time im milliseconds to wait.</param>
        ///// 
        /////<remarks></remarks>
        //private void FlashGeometry(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IRgbColor color, ESRI.ArcGIS.Display.IDisplay display, System.Int32 delay, IEnvelope envelope)
        //{
        //    if (geometry == null || color == null || display == null)
        //    {
        //        return;
        //    }

        //    display.StartDrawing(display.hDC, (System.Int16)ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit Cast

        //    switch (geometry.GeometryType)
        //    {
        //        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
        //            {
        //                //Set the flash geometry's symbol.
        //                ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new ESRI.ArcGIS.Display.SimpleFillSymbolClass();
        //                simpleFillSymbol.Color = color;
        //                ESRI.ArcGIS.Display.ISymbol symbol = simpleFillSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
        //                symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

        //                //Flash the input polygon geometry.
        //                display.SetSymbol(symbol);
        //                display.DrawPolygon(geometry);
        //                System.Threading.Thread.Sleep(delay);
        //                display.DrawPolygon(geometry);
        //                break;
        //            }

        //        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
        //            {
        //                //Set the flash geometry's symbol.
        //                ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
        //                simpleLineSymbol.Width = 4;
        //                simpleLineSymbol.Color = color;
        //                ESRI.ArcGIS.Display.ISymbol symbol = simpleLineSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
        //                symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

        //                //Flash the input polyline geometry.
        //                display.SetSymbol(symbol);
        //                display.DrawPolyline(geometry);
        //                System.Threading.Thread.Sleep(delay);
        //                display.DrawPolyline(geometry);
        //                break;
        //            }

        //        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
        //            {
        //                //Set the flash geometry's symbol.
        //                ESRI.ArcGIS.Display.ISimpleMarkerSymbol simpleMarkerSymbol = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
        //                simpleMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
        //                simpleMarkerSymbol.Size = 12;
        //                simpleMarkerSymbol.Color = color;
        //                ESRI.ArcGIS.Display.ISymbol markerSymbol = simpleMarkerSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
        //                markerSymbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

        //                ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
        //                simpleLineSymbol.Width = 1;
        //                simpleLineSymbol.Color = color;
        //                ESRI.ArcGIS.Display.ISymbol lineSymbol = simpleLineSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
        //                lineSymbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

        //                //Flash the input polygon geometry.
        //                display.SetSymbol(markerSymbol);
        //                display.SetSymbol(lineSymbol);

        //                DrawCrossHair(geometry, display, envelope, markerSymbol, lineSymbol);

        //                //Flash the input point geometry.
        //                display.SetSymbol(markerSymbol);
        //                display.DrawPoint(geometry);
        //                System.Threading.Thread.Sleep(delay);
        //                display.DrawPoint(geometry);
        //                break;
        //            }

        //        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
        //            {
        //                //Set the flash geometry's symbol.
        //                ESRI.ArcGIS.Display.ISimpleMarkerSymbol simpleMarkerSymbol = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
        //                simpleMarkerSymbol.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
        //                simpleMarkerSymbol.Size = 12;
        //                simpleMarkerSymbol.Color = color;
        //                ESRI.ArcGIS.Display.ISymbol symbol = simpleMarkerSymbol as ESRI.ArcGIS.Display.ISymbol; // Dynamic Cast
        //                symbol.ROP2 = ESRI.ArcGIS.Display.esriRasterOpCode.esriROPNotXOrPen;

        //                //Flash the input multipoint geometry.
        //                display.SetSymbol(symbol);
        //                display.DrawMultipoint(geometry);
        //                System.Threading.Thread.Sleep(delay);
        //                display.DrawMultipoint(geometry);
        //                break;
        //            }
        //    }

        //    display.FinishDrawing();
        //}

        //private static void DrawCrossHair(ESRI.ArcGIS.Geometry.IGeometry geometry, ESRI.ArcGIS.Display.IDisplay display, IEnvelope extent, ISymbol markerSymbol, ISymbol lineSymbol)
        //{
        //    var point = geometry as IPoint;
        //    var numSegments = 10;

        //    var latitudeMid = point.Y;//envelope.YMin + ((envelope.YMax - envelope.YMin) / 2);
        //    var longitudeMid = point.X;
        //    var leftLongSegment = (point.X - extent.XMin) / numSegments;
        //    var rightLongSegment = (extent.XMax - point.X) / numSegments;
        //    var topLatSegment = (extent.YMax - point.Y) / numSegments;
        //    var bottomLatSegment = (point.Y - extent.YMin) / numSegments;
        //    var fromLeftLong = extent.XMin;
        //    var fromRightLong = extent.XMax;
        //    var fromTopLat = extent.YMax;
        //    var fromBottomLat = extent.YMin;
        //    var av = (ArcMap.Application.Document as IMxDocument).ActiveView;

        //    var leftPolyline = new PolylineClass();
        //    var rightPolyline = new PolylineClass();
        //    var topPolyline = new PolylineClass();
        //    var bottomPolyline = new PolylineClass();

        //    leftPolyline.SpatialReference = geometry.SpatialReference;
        //    rightPolyline.SpatialReference = geometry.SpatialReference;
        //    topPolyline.SpatialReference = geometry.SpatialReference;
        //    bottomPolyline.SpatialReference = geometry.SpatialReference;

        //    var leftPC = leftPolyline as IPointCollection;
        //    var rightPC = rightPolyline as IPointCollection;
        //    var topPC = topPolyline as IPointCollection;
        //    var bottomPC = bottomPolyline as IPointCollection;

        //    leftPC.AddPoint(new PointClass() { X = fromLeftLong, Y = latitudeMid });
        //    rightPC.AddPoint(new PointClass() { X = fromRightLong, Y = latitudeMid });
        //    topPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromTopLat });
        //    bottomPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromBottomLat });

        //    for (int x = 1; x <= numSegments; x++)
        //    {
        //        //Flash the input polygon geometry.
        //        display.SetSymbol(markerSymbol);
        //        display.SetSymbol(lineSymbol);

        //        leftPC.AddPoint(new PointClass() { X = fromLeftLong + leftLongSegment * x, Y = latitudeMid });
        //        rightPC.AddPoint(new PointClass() { X = fromRightLong - rightLongSegment * x, Y = latitudeMid });
        //        topPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromTopLat - topLatSegment * x });
        //        bottomPC.AddPoint(new PointClass() { X = longitudeMid, Y = fromBottomLat + bottomLatSegment * x });

        //        // draw
        //        display.DrawPolyline(leftPolyline);
        //        display.DrawPolyline(rightPolyline);
        //        display.DrawPolyline(topPolyline);
        //        display.DrawPolyline(bottomPolyline);

        //        System.Threading.Thread.Sleep(15);
        //        display.FinishDrawing();
        //        av.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
        //        //av.Refresh();
        //        System.Windows.Forms.Application.DoEvents();
        //        display.StartDrawing(display.hDC, (System.Int16)ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit Cast
        //    }
        //}

        //internal void AddCollectionPoint(IPoint point)
        //{
        //    var color = new RgbColorClass() { Red = 255 } as IColor;
        //    var guid = AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, 7);
        //    var addInPoint = new AddInPoint() { Point = point, GUID = guid };
        //    CoordinateAddInPoints.Add(addInPoint);
        //}

        ///// <summary>
        ///// Adds a graphic element to the map graphics container
        ///// </summary>
        ///// <param name="geom">IGeometry</param>
        //private string AddGraphicToMap(IGeometry geom, IColor color, bool IsTempGraphic = false, esriSimpleMarkerStyle markerStyle = esriSimpleMarkerStyle.esriSMSCircle, int size = 5)
        //{
        //    if (geom == null || ArcMap.Document == null || ArcMap.Document.FocusMap == null)
        //        return string.Empty;

        //    IElement element = null;
        //    double width = 2.0;

        //    geom.Project(ArcMap.Document.FocusMap.SpatialReference);

        //    if (geom.GeometryType == esriGeometryType.esriGeometryPoint)
        //    {
        //        // Marker symbols
        //        var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;
        //        simpleMarkerSymbol.Color = color;
        //        simpleMarkerSymbol.Outline = false;
        //        simpleMarkerSymbol.OutlineColor = color;
        //        simpleMarkerSymbol.Size = size;
        //        simpleMarkerSymbol.Style = markerStyle;

        //        var markerElement = new MarkerElement() as IMarkerElement;
        //        markerElement.Symbol = simpleMarkerSymbol;
        //        element = markerElement as IElement;
        //    }
        //    else if (geom.GeometryType == esriGeometryType.esriGeometryPolyline)
        //    {
        //        // create graphic then add to map
        //        var le = new LineElementClass() as ILineElement;
        //        element = le as IElement;

        //        var lineSymbol = new SimpleLineSymbolClass();
        //        lineSymbol.Color = color;
        //        lineSymbol.Width = width;

        //        le.Symbol = lineSymbol;
        //    }
        //    else if (geom.GeometryType == esriGeometryType.esriGeometryPolygon)
        //    {
        //        // create graphic then add to map
        //        IPolygonElement pe = new PolygonElementClass() as IPolygonElement;
        //        element = pe as IElement;
        //        IFillShapeElement fe = pe as IFillShapeElement;

        //        var fillSymbol = new SimpleFillSymbolClass();
        //        RgbColor selectedColor = new RgbColorClass();
        //        selectedColor.Red = 0;
        //        selectedColor.Green = 0;
        //        selectedColor.Blue = 0;

        //        selectedColor.Transparency = (byte)0;
        //        fillSymbol.Color = selectedColor;

        //        fe.Symbol = fillSymbol;
        //    }

        //    if (element == null)
        //        return string.Empty;

        //    element.Geometry = geom;

        //    var mxdoc = ArcMap.Application.Document as IMxDocument;
        //    var av = mxdoc.FocusMap as IActiveView;
        //    var gc = av as IGraphicsContainer;

        //    // store guid
        //    var eprop = element as IElementProperties;
        //    eprop.Name = Guid.NewGuid().ToString();

        //    GraphicsList.Add(new AMGraphic(eprop.Name, geom, IsTempGraphic));

        //    gc.AddElement(element, 0);

        //    av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        //    //TODO check to see if this is still needed
        //    //RaisePropertyChanged(() => HasMapGraphics);

        //    return eprop.Name;
        //}

        //private void DeletePoints(List<AddInPoint> aiPoints)
        //{
        //    if (aiPoints == null || !aiPoints.Any())
        //        return;

        //    // remove graphics from map
        //    var guidList = aiPoints.Select(x => x.GUID).ToList();
        //    RemoveGraphics(guidList);

        //    foreach (var point in aiPoints)
        //    {
        //        CoordinateAddInPoints.Remove(point);
        //    }
        //}

        //private void RemoveGraphics(List<string> guidList)
        //{
        //    if (!guidList.Any())
        //        return;

        //    var mxdoc = ArcMap.Application.Document as IMxDocument;
        //    if (mxdoc == null)
        //        return;
        //    var av = mxdoc.FocusMap as IActiveView;
        //    if (av == null)
        //        return;
        //    var gc = av as IGraphicsContainer;
        //    if (gc == null)
        //        return;

        //    var graphics = GraphicsList.Where(g => guidList.Contains(g.UniqueId)).ToList();
        //    RemoveGraphics(gc, graphics);

        //    av.Refresh();
        //}

        /// <summary>
        /// Method used to remove graphics from the graphics container
        /// Elements are tagged with a GUID on the IElementProperties.Name property
        /// </summary>
        /// <param name="gc">map graphics container</param>
        /// <param name="list">list of GUIDs to remove</param>
        //internal void RemoveGraphics(IGraphicsContainer gc, List<AMGraphic> list)
        //{
        //    if (gc == null || !list.Any())
        //        return;

        //    var elementList = new List<IElement>();
        //    gc.Reset();
        //    var element = gc.Next();
        //    while (element != null)
        //    {
        //        var eleProps = element as IElementProperties;
        //        if (list.Any(g => g.UniqueId == eleProps.Name))
        //        {
        //            elementList.Add(element);
        //        }
        //        element = gc.Next();
        //    }

        //    foreach (var ele in elementList)
        //    {
        //        gc.DeleteElement(ele);
        //    }

        //    // remove from master graphics list
        //    foreach (var graphic in list)
        //    {
        //        if (GraphicsList.Contains(graphic))
        //            GraphicsList.Remove(graphic);
        //    }
        //    elementList.Clear();
        //    //TODO check to see if we still need this
        //    //RaisePropertyChanged(() => HasMapGraphics);
        //}
        #endregion older code
    }

    public class BoolToOppositeBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
