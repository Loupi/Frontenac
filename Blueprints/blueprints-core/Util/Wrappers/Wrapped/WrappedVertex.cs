using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedVertex : WrappedElement, IVertex
    {
        readonly IVertex _baseVertex;

        public WrappedVertex(IVertex baseVertex)
            : base(baseVertex)
        {
            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            return vertex != null ? new WrappedEdgeIterable(vertex.GetEdges(direction, labels)) : null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            return vertex != null ? new WrappedVertexIterable(vertex.GetVertices(direction, labels)) : null;
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new WrappedEdgeIterable(t.Edges()),
                                          t => new WrappedVertexIterable(t.Vertices()));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            if (vertex is WrappedVertex)
            {
                var vertex1 = BaseElement as IVertex;
                if (vertex1 != null)
                    return new WrappedEdge(vertex1.AddEdge(label, (vertex as WrappedVertex).GetBaseVertex()));
            }
            else
            {
                var vertex2 = BaseElement as IVertex;
                if (vertex2 != null)
                    return new WrappedEdge(vertex2.AddEdge(label, vertex));
            }
            return null;
        }

        public IVertex GetBaseVertex()
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return _baseVertex;
        }
    }
}
