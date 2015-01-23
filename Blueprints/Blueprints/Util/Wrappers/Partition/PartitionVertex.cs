using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        public PartitionVertex(IVertex vertex, PartitionGraph innerTinkerGraĥ)
            : base(vertex, innerTinkerGraĥ)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(innerTinkerGraĥ != null);

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new PartitionEdgeIterable(Vertex.GetEdges(direction, labels), PartitionInnerTinkerGraĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new PartitionVertexIterable(Vertex.GetVertices(direction, labels), PartitionInnerTinkerGraĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new PartitionEdgeIterable(t.Edges(), PartitionInnerTinkerGraĥ),
                                          t => new PartitionVertexIterable(t.Vertices(), PartitionInnerTinkerGraĥ));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return Graph.AddEdge(null, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}