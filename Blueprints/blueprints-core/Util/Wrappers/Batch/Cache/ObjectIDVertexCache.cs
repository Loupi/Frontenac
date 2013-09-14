using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class ObjectIdVertexCache : IVertexCache
    {
        private const int InitialCapacity = 1000;

        private Dictionary<object, object> _map;

        public ObjectIdVertexCache()
        {
            _map = new Dictionary<object, object>(InitialCapacity);
        }

        public object GetEntry(object externalId)
        {
            return _map.Get(externalId);
        }

        public void Set(IVertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            _map[externalId] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _map.ContainsKey(externalId);
        }

        public void NewTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is IVertex ? (t.Value as IVertex).Id : t.Value);
        }
    }
}