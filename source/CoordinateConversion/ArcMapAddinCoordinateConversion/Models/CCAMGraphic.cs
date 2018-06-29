using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinCoordinateConversion.Models
{
    public class CCAMGraphic
    {
        public AMGraphic MapPoint { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}
