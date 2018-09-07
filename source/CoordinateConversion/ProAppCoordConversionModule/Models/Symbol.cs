using ArcGIS.Desktop.Mapping;
using System.Windows.Media.Imaging;

namespace ProAppCoordConversionModule.Models
{
    public class Symbol
    {
        public BitmapImage SymbolImage { get; set; }
        public string SymbolText { get; set; }
        public SymbolStyleItem SymbolItem { get; set; }
    }
}
