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

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable((_BaseElement as Vertex).GetEdges(direction, labels));
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable((_BaseElement as Vertex).GetVertices(direction, labels));
        }

        public Edge AddEdge(string label, Vertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public VertexQuery Query()
        {
            return new WrapperVertexQuery((_BaseElement as Vertex).Query(),
                t => new ReadOnlyEdgeIterable(t.Edges()),
                t => new ReadOnlyVertexIterable(t.Vertices()));
        }
    }
}
