using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IEdge))]
    public abstract class EdgeContract : IEdge
    {
        public IVertex GetVertex(Direction direction)
        {
            Contract.Requires(direction != Direction.Both);
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return null;
        }

        public string Label
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return null;
            }
        }

        public abstract object GetProperty(string key);
        public abstract IEnumerable<string> GetPropertyKeys();
        public abstract void SetProperty(string key, object value);
        public abstract object RemoveProperty(string key);
        public abstract void Remove();
        public abstract object Id { get; }
    }
}