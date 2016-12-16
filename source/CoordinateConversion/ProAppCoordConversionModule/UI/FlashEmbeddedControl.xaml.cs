using CoordinateConversionLibrary.Helpers;
using System.Windows.Controls;

namespace ProAppCoordConversionModule.UI
{
    /// <summary>
    /// Interaction logic for FlashEmbeddedControl.xaml
    /// </summary>
    public partial class FlashEmbeddedControl : UserControl
    {
        public FlashEmbeddedControl()
        {
            InitializeComponent();
        }

        private void Storyboard_Completed(object sender, System.EventArgs e)
        {
            Mediator.NotifyColleagues("FLASH_COMPLETED", null);
        }
    }
}
