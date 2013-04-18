using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, Vertex
    {
        public IdVertex(Vertex baseVertex, IdGraph idGraph)
            : base(baseVertex, idGraph, idGraph.getSupportVertexIds())
        {

        }

        public Vertex getBaseVertex()
        {
            return this.baseElement as Vertex;
        }

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable((baseElement as Vertex).getEdges(direction, labels), idGraph);
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable((baseElement as Vertex).getVertices(direction, labels), idGraph);
        }

        public VertexQuery query()
        {
            return new WrapperVertexQuery((baseElement as Vertex).query(),
                t => new IdEdgeIterable(t.edges(), idGraph),
                t => new IdVertexIterable(t.vertices(), idGraph));
        }

        public Edge addEdge(string label, Vertex vertex)
        {
            return idGraph.addEdge(null, this, vertex, label);
        }

        public override string ToString()
        {
            return StringFactory.vertexString(this);
        }
    }
}
