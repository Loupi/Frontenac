using System.Collections.Generic;

namespace Frontenac.Grave.Entities
{
    public interface IContributor : INamedEntity
    {
        int? Age { get; set; }
        string Language { get; set; }
        IEnumerable<KeyValuePair<IWeightedEntity, IContributor>> Knows { get; set; }
        IEnumerable<KeyValuePair<IWeightedEntity, IContributor>> Created { get; set; }
    }
}
