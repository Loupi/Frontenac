using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        public PartitionVertex(IVertex vertex, PartitionGraph graph)
            : base(vertex, graph)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(graph != null);

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new PartitionEdgeIterable(Vertex.GetEdges(direction, labels), PartitionGraph);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new PartitionVertexIterable(Vertex.GetVertices(direction, labels), PartitionGraph);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new PartitionEdgeIterable(t.Edges(), PartitionGraph),
                                          t => new PartitionVertexIterable(t.Vertices(), PartitionGraph));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return Graph.AddEdge(null, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}