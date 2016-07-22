using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinCoordinateConversion.Helpers
{
    public class ArcMapHelpers
    {
        public static ISpatialReference GetGCS_WGS_1984_SR()
        {
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            return geographicCS as ISpatialReference;
        }
        public static ISpatialReference GetSR(int type)
        {
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.
            try
            {
                IGeographicCoordinateSystem geographicCS = srFact.CreateGeographicCoordinateSystem(type);

                return geographicCS as ISpatialReference;
            }
            catch (Exception ex)
            {
                // do nothing
            }


            try
            {
                IProjectedCoordinateSystem projectCS = srFact.CreateProjectedCoordinateSystem(type);

                return projectCS as ISpatialReference;
            }
            catch (Exception ex)
            {
                // do nothing
            }

            return null;
        }

    }
}
