using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        public PartitionVertex(IVertex vertex, PartitionGraph graph)
            : base(vertex, graph)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

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

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return Vertex.GetVertices(direction, label, ids);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return Vertex.GetNbEdges(direction, label);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new PartitionEdgeIterable(t.Edges(), PartitionGraph),
                                          t => new PartitionVertexIterable(t.Vertices(), PartitionGraph));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return Graph.AddEdge(id, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}