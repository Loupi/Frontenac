using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedVertex : WrappedElement, Vertex
    {
        public WrappedVertex(Vertex baseVertex)
            : base(baseVertex)
        {

        }

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            return new WrappedEdgeIterable((_BaseElement as Vertex).GetEdges(direction, labels));
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new WrappedVertexIterable((_BaseElement as Vertex).GetVertices(direction, labels));
        }

        public VertexQuery Query()
        {
            return new WrapperVertexQuery((_BaseElement as Vertex).Query(),
                t => new WrappedEdgeIterable(t.Edges()),
                t => new WrappedVertexIterable(t.Vertices()));
        }

        public Edge AddEdge(string label, Vertex vertex)
        {
            if (vertex is WrappedVertex)
                return new WrappedEdge((_BaseElement as Vertex).AddEdge(label, (vertex as WrappedVertex).GetBaseVertex()));
            else
                return new WrappedEdge((_BaseElement as Vertex).AddEdge(label, vertex));
        }

        public Vertex GetBaseVertex()
        {
            return this._BaseElement as Vertex;
        }
    }
}
