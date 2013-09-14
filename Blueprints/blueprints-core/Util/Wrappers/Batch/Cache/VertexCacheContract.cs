using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    [ContractClassFor(typeof (IVertexCache))]
    public abstract class VertexCacheContract : IVertexCache
    {
        public object GetEntry(object externalId)
        {
            Contract.Requires(externalId != null);
            return null;
        }

        public void Set(IVertex vertex, object externalId)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(externalId != null);
        }

        public void SetId(object vertexId, object externalId)
        {
            Contract.Requires(vertexId != null);
            Contract.Requires(externalId != null);
        }

        public bool Contains(object externalId)
        {
            Contract.Requires(externalId != null);
            return default(bool);
        }

        public void NewTransaction()
        {
        }
    }
}