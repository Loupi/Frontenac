using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class LongIdVertexCache : IVertexCache
    {
        private const int InitialCapacity = 1000;

        private IDictionary<long, object> _map;

        public LongIdVertexCache()
        {
            _map = new Dictionary<long, object>(InitialCapacity);
        }

        public object GetEntry(object externalId)
        {
            var id = GetId(externalId);
            return _map.Get(id);
        }

        public void Set(IVertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            var id = GetId(externalId);
            _map[id] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _map.ContainsKey(GetId(externalId));
        }

        public void NewTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is IVertex ? (t.Value as IVertex).Id : t.Value);
        }

        private static long GetId(object externalId)
        {
            Contract.Requires(Portability.IsNumber(externalId));

            return Convert.ToInt64(externalId);
        }
    }
}