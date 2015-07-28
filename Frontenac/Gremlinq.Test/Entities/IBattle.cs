using System.Collections.Generic;
using Frontenac.Blueprints.Geo;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IBattle
    {
        int Time { get; set; }
        GeoPoint Place { get; set; }

        ICharacter Opponent { get; }
        IEnumerable<ICharacter> BothOpponent { get; }
    }
}