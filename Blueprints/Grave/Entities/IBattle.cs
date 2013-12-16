using Frontenac.Grave.Geo;

namespace Frontenac.Grave.Entities
{
    public interface IBattle
    {
        int Time { get; set; }
        GeoPoint Place { get; set; }

        ICharacter In { get; }
        ICharacter Out { get; }
    }
}