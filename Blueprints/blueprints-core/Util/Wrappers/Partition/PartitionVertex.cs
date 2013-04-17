using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, Vertex
    {
        public PartitionVertex(Vertex baseVertex, PartitionGraph graph)
            : base(baseVertex, graph)
        {

        }

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            return new PartitionEdgeIterable((_BaseElement as Vertex).GetEdges(direction, labels), _Graph);
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new PartitionVertexIterable((_BaseElement as Vertex).GetVertices(direction, labels), _Graph);
        }

        public VertexQuery Query()
        {
            return new WrapperVertexQuery((_BaseElement as Vertex).Query(),
                t => new PartitionEdgeIterable(t.Edges(), _Graph),
                t => new PartitionVertexIterable(t.Vertices(), _Graph));
        }

        public Edge AddEdge(string label, Vertex vertex)
        {
            return _Graph.AddEdge(null, this, vertex, label);
        }

        public Vertex GetBaseVertex()
        {
            return this._BaseElement as Vertex;
        }
    }
}
