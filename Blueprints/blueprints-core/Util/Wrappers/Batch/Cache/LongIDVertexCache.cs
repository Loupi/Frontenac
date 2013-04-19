using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class LongIDVertexCache : VertexCache
    {
        const int INITIAL_CAPACITY = 1000;

        IDictionary<long, object> _map;

        public LongIDVertexCache()
        {
            _map = new Dictionary<long, object>(INITIAL_CAPACITY);
        }

        static long getId(object externalID)
        {
            if (!(Portability.isNumber(externalID))) throw new ArgumentException("Number expected.");
            return Convert.ToInt64(externalID);
        }

        public object getEntry(object externalId)
        {
            long id = getId(externalId);
            return _map.get(id);
        }

        public void set(Vertex vertex, object externalId)
        {
            setId(vertex, externalId);
        }

        public void setId(object vertexId, object externalId)
        {
            long id = getId(externalId);
            _map[id] = vertexId;
        }

        public bool contains(object externalId)
        {
            return _map.ContainsKey(getId(externalId));
        }

        public void newTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).getId() : t.Value);
        }
    }
}
