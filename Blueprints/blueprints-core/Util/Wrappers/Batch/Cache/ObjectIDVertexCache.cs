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

        Dictionary<object, object> _map;

        public ObjectIDVertexCache()
        {
            _map = new Dictionary<object, object>(INITIAL_CAPACITY);
        }

        public object getEntry(object externalId)
        {
            return _map.get(externalId);
        }

        public void set(Vertex vertex, object externalId)
        {
            setId(vertex, externalId);
        }

        public void setId(object vertexId, object externalId)
        {
            _map[externalId] = vertexId;
        }

        public bool contains(object externalId)
        {
            return _map.ContainsKey(externalId);
        }

        public void newTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).getId() : t.Value);
        }
    }
}
