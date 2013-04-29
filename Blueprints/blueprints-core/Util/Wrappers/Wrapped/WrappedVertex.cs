using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedVertex : WrappedElement, IVertex
    {
        public WrappedVertex(IVertex baseVertex)
            : base(baseVertex)
        {

        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new WrappedEdgeIterable(vertex.GetEdges(direction, labels));
            return null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new WrappedVertexIterable(vertex.GetVertices(direction, labels));
            return null;
        }

        public IVertexQuery Query()
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new WrapperVertexQuery(vertex.Query(),
                                              t => new WrappedEdgeIterable(t.Edges()),
                                              t => new WrappedVertexIterable(t.Vertices()));
            return null;
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
            return BaseElement as IVertex;
        }
    }
}
