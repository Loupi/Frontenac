using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class ObjectIDVertexCache : VertexCache
    {
        const int INITIAL_CAPACITY = 1000;

        Dictionary<object, object> _Map;

        public ObjectIDVertexCache()
        {
            _Map = new Dictionary<object, object>(INITIAL_CAPACITY);
        }

        public object GetEntry(object externalId)
        {
            return _Map.Get(externalId);
        }

        public void Set(Vertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            _Map[externalId] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _Map.ContainsKey(externalId);
        }

        public void NewTransaction()
        {
            _Map = _Map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).GetId() : t.Value);
        }
    }
}
