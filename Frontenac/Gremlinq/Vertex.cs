using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util.Wrappers.Wrapped;

namespace Frontenac.Gremlinq
{
    public class Vertex<TModel> : WrappedVertex, IVertex<TModel>
    {
        internal Vertex(IVertex vertex, TModel model):base(vertex)
        {
            Contract.Requires(vertex != null);

            Model = model;
        }

        public TModel Model { get; private set; }
    }
}