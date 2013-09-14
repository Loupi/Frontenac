using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public PartitionVertex(IVertex baseVertex, PartitionGraph graph)
            : base(baseVertex, graph)
        {
            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            return vertex != null ? new PartitionEdgeIterable(vertex.GetEdges(direction, labels), Graph) : null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            return vertex != null ? new PartitionVertexIterable(vertex.GetVertices(direction, labels), Graph) : null;
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new PartitionEdgeIterable(t.Edges(), Graph),
                                          t => new PartitionVertexIterable(t.Vertices(), Graph));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return Graph.AddEdge(null, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return _baseVertex;
        }
    }
}