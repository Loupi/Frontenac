using System;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util.Wrappers.Wrapped;

namespace Frontenac.Gremlinq
{
    public class Edge<TModel> : WrappedEdge, IEdge<TModel>
    {
        public Edge(IEdge edge, TModel model) : base(edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            Model = model;
        }

        public TModel Model { get; private set; }
    }
}