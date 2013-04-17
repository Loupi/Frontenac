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

        IDictionary<long, object> _Map;

        public LongIDVertexCache()
        {
            _Map = new Dictionary<long, object>(INITIAL_CAPACITY);
        }

        static long GetID(object externalID)
        {
            if (!(Portability.IsNumeric(externalID))) throw new ArgumentException("Number expected.");
            return Convert.ToInt64(externalID);
        }

        public object GetEntry(object externalId)
        {
            long id = GetID(externalId);
            return _Map.Get(id);
        }

        public void Set(Vertex vertex, object externalId)
        {
            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            long id = GetID(externalId);
            _Map[id] = vertexId;
        }

        public bool Contains(object externalId)
        {
            return _Map.ContainsKey(GetID(externalId));
        }

        public void NewTransaction()
        {
            _Map = _Map.ToDictionary(t => t.Key, t => t.Value is Vertex ? (t.Value as Vertex).GetId() : t.Value);
        }
    }
}
