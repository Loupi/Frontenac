using System.Collections.Generic;

namespace Frontenac.Grave.Entities
{
    public interface ICharacter : INamedEntity
    {
        ICharacter Father { get; set; }
        ICharacter Mother { get; set; }
        IEnumerable<ICharacter> Brother { get; set; }
        IEnumerable<IMonster> Pet { get; set; }
        KeyValuePair<ILive, ILocation> Lives { get; set; }
        IEnumerable<KeyValuePair<IBattle, ICharacter>> Battled { get; set; }
    }
}