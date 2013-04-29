using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        public PartitionVertex(IVertex baseVertex, PartitionGraph graph)
            : base(baseVertex, graph)
        {

        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new PartitionEdgeIterable(vertex.GetEdges(direction, labels), Graph);
            return null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new PartitionVertexIterable(vertex.GetVertices(direction, labels), Graph);
            return null;
        }

        public IVertexQuery Query()
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new WrapperVertexQuery(vertex.Query(),
                                              t => new PartitionEdgeIterable(t.Edges(), Graph),
                                              t => new PartitionVertexIterable(t.Vertices(), Graph));
            return null;
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return Graph.AddEdge(null, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            return BaseElement as IVertex;
        }
    }
}
