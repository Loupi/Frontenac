using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util.Wrappers.Wrapped;

namespace Frontenac.Gremlinq
{
    public class Edge<TModel> : WrappedEdge, IEdge<TModel>
    {
        public Edge(IEdge edge, TModel model) : base(edge)
        {
            Contract.Requires(edge != null);

            Model = model;
        }

        public TModel Model { get; }
    }
}