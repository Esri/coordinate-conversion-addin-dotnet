using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace ArcMapAddinCoordinateTool
{
    public class CoordinateToolButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public CoordinateToolButton()
        {
        }

        protected override void OnClick()
        {
            ArcMap.Application.CurrentTool = null;

            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.DockableWindowCoordinateTool;

            IDockableWindow dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
            dockWindow.Show(true);
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
