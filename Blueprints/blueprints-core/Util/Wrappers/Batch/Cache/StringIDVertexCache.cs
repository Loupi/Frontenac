using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class StringIdVertexCache : IVertexCache
    {
        const int InitialCapacity = 1000;

        Dictionary<string, object> _map;
        readonly StringCompression _compression;

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
            _map = _map.ToDictionary(t => t.Key, t => t.Value is IVertex ? (t.Value as IVertex).Id : t.Value);
        }
    }
}
