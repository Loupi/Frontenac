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
            : base(baseVertex, idGraph, idGraph.GetSupportVertexIds())
        {

        }

        public Vertex GetBaseVertex()
        {
            return this._BaseElement as Vertex;
        }

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable((_BaseElement as Vertex).GetEdges(direction, labels), _IdGraph);
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable((_BaseElement as Vertex).GetVertices(direction, labels), _IdGraph);
        }

        public VertexQuery Query()
        {
            return new WrapperVertexQuery((_BaseElement as Vertex).Query(),
                t => new IdEdgeIterable(t.Edges(), _IdGraph),
                t => new IdVertexIterable(t.Vertices(), _IdGraph));
        }

        public Edge AddEdge(string label, Vertex vertex)
        {
            return _IdGraph.AddEdge(null, this, vertex, label);
        }

        public override string ToString()
        {
            return StringFactory.VertexString(this);
        }
    }
}
