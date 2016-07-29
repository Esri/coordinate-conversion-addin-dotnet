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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.Helpers;
using ArcGIS.Core.Geometry;
using CoordinateConversionLibrary.ViewModels;
using System.ComponentModel;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Core.Data;
using System.Text.RegularExpressions;
using ProAppCoordConversionModule.ViewModels;
using System.Windows.Controls;
using CoordinateConversionLibrary;

namespace ProAppCoordConversionModule
{
    internal class CoordinateConversionDockpaneViewModel : DockPane
    {
        protected CoordinateConversionDockpaneViewModel() 
        {
            ConvertTabView = new CCConvertTabView();
            ConvertTabView.DataContext = new ProConvertTabViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new ProCollectTabViewModel();

            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
        }

        ~CoordinateConversionDockpaneViewModel()
        {
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
        }

        private const string _dockPaneID = "ProAppCoordConversionModule_CoordinateConversionDockpane";
        
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
                Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
                //TODO let the other viewmodels determine what to do when tab selection changes
                if (tabItem.Header.ToString() == CoordinateConversionLibrary.Properties.Resources.HeaderCollect)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Collect);
                else
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Convert);
            }
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

        private object _lock = new object();
        private async void OnSelectionChanged(MapSelectionChangedEventArgs obj)
        {
            if (MapView.Active.Map != null && obj.Selection.Count == 1)
            {
                var fl = obj.Selection.FirstOrDefault().Key as FeatureLayer;
                if (fl == null || fl.SelectionCount != 1 || fl.ShapeType != esriGeometryType.esriGeometryPoint)
                    return;

                var pointd = await QueuedTask.Run(() =>
                {
                    try
                    {
                        var SelectedOID = fl.GetSelection().GetObjectIDs().FirstOrDefault();
                        if (SelectedOID < 0)
                            return null;

                        var SelectedLayer = fl as BasicFeatureLayer;

                        var oidField = SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                        var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, SelectedOID) };
                        var cursor = SelectedLayer.Search(qf);
                        Row row = null;

                        if (cursor.MoveNext())
                            row = cursor.Current;

                        if (row == null)
                            return null;

                        var fields = row.GetFields();
                        lock (_lock)
                        {
                            foreach (ArcGIS.Core.Data.Field field in fields)
                            {
                                if (field.FieldType == FieldType.Geometry)
                                {
                                    // have mappoint here
                                    var val = row[field.Name];
                                    if (val is MapPoint)
                                    {
                                        var temp = val as MapPoint;
                                        return temp;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    return null;
                });

                if(pointd != null)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NewMapPointSelection, pointd);
            }
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CoordinateConversionDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            CoordinateConversionDockpaneViewModel.Show();
        }
    }
}
