namespace Frontenac.Blueprints.Geo
{
    public class GeoRectangle : IGeoShape
    {
        public GeoRectangle(GeoPoint topLeft, GeoPoint rightBottom)
        {
            TopLeft = topLeft;
            BottomRight = rightBottom;
        }

        public GeoRectangle(double minX, double maxX, double minY, double maxY)
        {
            TopLeft = new GeoPoint(minX, maxY);
            BottomRight = new GeoPoint(maxX, minY);
        }

        public GeoPoint TopLeft { get; set; }
        public GeoPoint BottomRight { get; set; }
    }
}
