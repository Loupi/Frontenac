using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof(IVertex))]
    public abstract class VertexContract : IVertex
    {
        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);
            return null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);
            return null;
        }

        public IVertexQuery Query()
        {
            Contract.Ensures(Contract.Result<IVertexQuery>() != null);
            return null;
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return null;
        }

        public abstract object GetProperty(string key);
        public abstract IEnumerable<string> GetPropertyKeys();
        public abstract void SetProperty(string key, object value);
        public abstract object RemoveProperty(string key);
        public abstract void Remove();
        public abstract object Id { get; }
    }
}
