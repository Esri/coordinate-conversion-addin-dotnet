using ProAppCoordConversionModule.Common;
using System.Windows.Controls;

namespace ProAppCoordConversionModule.Views
{
    /// <summary>
    /// Interaction logic for ProEditPropertiesView.xaml
    /// </summary>
    public partial class ProEditPropertiesView : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ProEditPropertiesView()
        {
            InitializeComponent();
            /*
             To avoid pro crash issue when you select custom format and select point from point tool
             */
            InputFormatHelper inputFormatHelper = new InputFormatHelper();
        }
    }
}
