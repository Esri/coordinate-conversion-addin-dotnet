using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordConversionModule.Models
{
    public class CCProGraphic
    {
        public MapPoint MapPoint { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}
