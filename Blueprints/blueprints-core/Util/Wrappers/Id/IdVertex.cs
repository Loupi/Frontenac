using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        readonly IVertex _baseVertex;

        public IdVertex(IVertex baseVertex, IdGraph idGraph)
            : base(baseVertex, idGraph, idGraph.GetSupportVertexIds())
        {
            _baseVertex = baseVertex;
        }

        public IVertex GetBaseVertex()
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return _baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(_baseVertex.GetEdges(direction, labels), IdGraph);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable(_baseVertex.GetVertices(direction, labels), IdGraph);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                t => new IdEdgeIterable(t.Edges(), IdGraph),
                t => new IdVertexIterable(t.Vertices(), IdGraph));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return IdGraph.AddEdge(null, this, vertex, label);
        }

        public override string ToString()
        {
            return StringFactory.VertexString(this);
        }
    }
}
