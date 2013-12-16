﻿namespace Frontenac.Grave.Geo
{
    public class GeoPoint : IGeoShape
    {
        public GeoPoint()
        {
            
        }

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
