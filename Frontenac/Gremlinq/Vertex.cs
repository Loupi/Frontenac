using System;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util.Wrappers.Wrapped;

namespace Frontenac.Gremlinq
{
    public class Vertex<TModel> : WrappedVertex, IVertex<TModel>
    {
        internal Vertex(IVertex vertex, TModel model):base(vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            Model = model;
        }

        public TModel Model { get; }
    }
}