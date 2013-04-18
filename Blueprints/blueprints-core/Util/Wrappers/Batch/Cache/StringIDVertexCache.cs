using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class StringIDVertexCache : VertexCache
    {
        const int INITIAL_CAPACITY = 1000;

        Dictionary<string, object> _map;
        readonly StringCompression _compression;

        public StringIDVertexCache(StringCompression compression)
        {
            if (compression == null) throw new ArgumentNullException("compression");
            _compression = compression;
            _map = new Dictionary<string, object>(INITIAL_CAPACITY);
        }

        public StringIDVertexCache()
            : this(StringCompression.NO_COMPRESSION)
        {

        }

        public object getEntry(object externalId)
        {
            string id = _compression.compress(externalId.ToString());
            return _map.get(id);
        }

        public void set(Vertex vertex, object externalId)
        {
            setId(vertex, externalId);
        }

        public void setId(object vertexId, object externalId)
        {
            string id = _compression.compress(externalId.ToString());
            _map[id] = vertexId;
        }

        public bool contains(object externalId)
        {
            return _map.ContainsKey(_compression.compress(externalId.ToString()));
        }

        public void newTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).getId() : t.Value);
        }
    }
}
