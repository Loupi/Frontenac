using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class StringIdVertexCache : IVertexCache
    {
        private const int InitialCapacity = 1000;

        private readonly StringCompression _compression;
        private Dictionary<string, object> _map;

        public StringIdVertexCache(StringCompression compression)
        {
            Contract.Requires(compression != null);

            _compression = compression;
            _map = new Dictionary<string, object>(InitialCapacity);
        }

        public StringIdVertexCache()
            : this(StringCompression.NoCompression)
        {
        }

        public object GetEntry(object externalId)
        {
            var id = _compression.Compress(externalId.ToString());
            return _map.Get(id);
        }

        public void Set(IVertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            var id = _compression.Compress(externalId.ToString());
            _map[id] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _map.ContainsKey(_compression.Compress(externalId.ToString()));
        }

        public void NewTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is IVertex ? ((IVertex) t.Value).Id : t.Value);
        }
    }
}