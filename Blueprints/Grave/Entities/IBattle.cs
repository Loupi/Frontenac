using Grave.Geo;

namespace Grave.Entities
{
    public interface IBattle
    {
        int Time { get; set; }
        GeoPoint Place { get; set; }

        ICharacter In { get; }
        ICharacter Out { get; }
    }
}