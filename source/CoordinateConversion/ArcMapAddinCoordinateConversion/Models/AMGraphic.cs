using ESRI.ArcGIS.Geometry;

namespace ArcMapAddinCoordinateConversion.Models
{
    public class AMGraphic
    {
        public AMGraphic(string _uniqueid, IGeometry _geometry, bool _isTemp = false)
        {
            UniqueId = _uniqueid;
            Geometry = _geometry;
            IsTemp = _isTemp;
        }

        // properties   

        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public IGeometry Geometry { get; set; }

        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp { get; set; }

    }
}
