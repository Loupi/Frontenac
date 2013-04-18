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

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            return new WrappedEdgeIterable((baseElement as Vertex).getEdges(direction, labels));
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new WrappedVertexIterable((baseElement as Vertex).getVertices(direction, labels));
        }

        public VertexQuery query()
        {
            return new WrapperVertexQuery((baseElement as Vertex).query(),
                t => new WrappedEdgeIterable(t.edges()),
                t => new WrappedVertexIterable(t.vertices()));
        }

        public Edge addEdge(string label, Vertex vertex)
        {
            if (vertex is WrappedVertex)
                return new WrappedEdge((baseElement as Vertex).addEdge(label, (vertex as WrappedVertex).getBaseVertex()));
            else
                return new WrappedEdge((baseElement as Vertex).addEdge(label, vertex));
        }

        public Vertex getBaseVertex()
        {
            return this.baseElement as Vertex;
        }
    }
}
