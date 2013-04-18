using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, Vertex
    {
        public ReadOnlyVertex(Vertex baseVertex)
            : base(baseVertex)
        {

        }

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable((baseElement as Vertex).getEdges(direction, labels));
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable((baseElement as Vertex).getVertices(direction, labels));
        }

        public Edge addEdge(string label, Vertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public VertexQuery query()
        {
            return new WrapperVertexQuery((baseElement as Vertex).query(),
                t => new ReadOnlyEdgeIterable(t.edges()),
                t => new ReadOnlyVertexIterable(t.vertices()));
        }
    }
}
