using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Impls.TG
{
    [ContractClassFor(typeof(ITinkerStorage))]
    public abstract class TinkerStorageContract : ITinkerStorage
    {
        public TinkerGraph Load(string directory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            Contract.Ensures(Contract.Result<TinkerGraph>() != null);
            return null;
        }

        public void Save(TinkerGraph graph, string directory)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
        }
    }
}
