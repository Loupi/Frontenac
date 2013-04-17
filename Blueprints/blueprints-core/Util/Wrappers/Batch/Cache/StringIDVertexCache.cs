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

        Dictionary<string, object> _Map;
        readonly StringCompression _Compression;

        public StringIDVertexCache(StringCompression compression)
        {
            if (compression == null) throw new ArgumentNullException("compression");
            _Compression = compression;
            _Map = new Dictionary<string, object>(INITIAL_CAPACITY);
        }

        public StringIDVertexCache()
            : this(StringCompression.NO_COMPRESSION)
        {

        }

        public object GetEntry(object externalId)
        {
            string id = _Compression.Compress(externalId.ToString());
            return _Map.Get(id);
        }

        public void Set(Vertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            string id = _Compression.Compress(externalId.ToString());
            _Map[id] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _Map.ContainsKey(_Compression.Compress(externalId.ToString()));
        }

        public void NewTransaction()
        {
            _Map = _Map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).GetId() : t.Value);
        }
    }
}
