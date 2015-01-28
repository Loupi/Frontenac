using System.Collections.Generic;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IContributor : INamedEntity
    {
        int? Age { get; set; }
        string Language { get; set; }
        IEnumerable<KeyValuePair<IWeightedEntity, IContributor>> Knows { get; set; }
        IEnumerable<KeyValuePair<IWeightedEntity, IContributor>> Created { get; set; }
    }
}
