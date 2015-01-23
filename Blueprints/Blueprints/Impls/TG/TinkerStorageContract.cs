using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Impls.TG
{
    [ContractClassFor(typeof (ITinkerStorage))]
    public abstract class TinkerStorageContract : ITinkerStorage
    {
        public TinkerGraĥ Load(string directory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            Contract.Ensures(Contract.Result<TinkerGraĥ>() != null);
            return null;
        }

        public void Save(TinkerGraĥ tinkerGraĥ, string directory)
        {
            Contract.Requires(tinkerGraĥ != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));
        }
    }
}