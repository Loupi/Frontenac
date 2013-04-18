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

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            return new PartitionEdgeIterable((baseElement as Vertex).getEdges(direction, labels), graph);
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new PartitionVertexIterable((baseElement as Vertex).getVertices(direction, labels), graph);
        }

        public VertexQuery query()
        {
            return new WrapperVertexQuery((baseElement as Vertex).query(),
                t => new PartitionEdgeIterable(t.edges(), graph),
                t => new PartitionVertexIterable(t.vertices(), graph));
        }

        public Edge addEdge(string label, Vertex vertex)
        {
            return graph.addEdge(null, this, vertex, label);
        }

        public Vertex getBaseVertex()
        {
            return this.baseElement as Vertex;
        }
    }
}
