using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CoordinateConversionLibrary.Views;

namespace ProAppCoordConversionModule
{
    internal class PropertiesButton : Button
    {
        protected override void OnClick()
        {
            var dlg = new EditPropertiesView();

            dlg.ShowDialog();
        }
    }
}
