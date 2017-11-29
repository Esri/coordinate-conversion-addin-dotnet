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
using System.Windows.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ArcMapAddinCoordinateConversion.ViewModels;
using CoordinateConversionLibrary.Helpers;

namespace ArcMapAddinCoordinateConversion
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains WPF user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class DockableWindowCoordinateConversion : UserControl
    {
        public DockableWindowCoordinateConversion()
        {
            InitializeComponent();

            if (ArcMap.Events != null)
            {
                ArcMap.Events.NewDocument += ArcMap_NewOpenDocument;
                ArcMap.Events.OpenDocument += ArcMap_NewOpenDocument;
            }
        }

        IActiveViewEvents_Event avEvents = null;
        
        private void ArcMap_NewOpenDocument()
        {
            if(avEvents != null)
            {
                avEvents.SelectionChanged -= OnSelectionChanged;
                avEvents = null;
            }
            avEvents = ArcMap.Document.ActiveView as IActiveViewEvents_Event;
            avEvents.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (ArcMap.Document.FocusMap.SelectionCount > 0)
            {
                for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++ )
                {
                    if(ArcMap.Document.FocusMap.get_Layer(i) is IFeatureLayer)
                    {
                        var fl = ArcMap.Document.FocusMap.get_Layer(i) as IFeatureLayer;

                        var fselection = fl as IFeatureSelection;
                        if (fselection == null)
                            continue;

                        if(fselection.SelectionSet.Count == 1)
                        {
                            ICursor cursor;
                            fselection.SelectionSet.Search(null, false, out cursor);

                            var fc = cursor as IFeatureCursor;
                            var f = fc.NextFeature();

                            if(f != null)
                            {
                                if(f.Shape is IPoint)
                                {
                                    var point = f.Shape as IPoint;
                                    if(point != null)
                                    {
                                        var tempX = point.X;
                                        var tempY = point.Y;

                                        Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.NewMapPointSelection, point);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private System.Windows.Forms.Integration.ElementHost m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new System.Windows.Forms.Integration.ElementHost();
                m_windowUI.Child = new DockableWindowCoordinateConversion();
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose();

                base.Dispose(disposing);
            }

        }
    }
}
