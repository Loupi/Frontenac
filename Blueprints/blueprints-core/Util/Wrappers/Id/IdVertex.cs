using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        public IdVertex(IVertex baseVertex, IdGraph idGraph)
            : base(baseVertex, idGraph, idGraph.GetSupportVertexIds())
        {

        }

        public IVertex GetBaseVertex()
        {
            return BaseElement as IVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(((IVertex) BaseElement).GetEdges(direction, labels), IdGraph);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable(((IVertex) BaseElement).GetVertices(direction, labels), IdGraph);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(((IVertex) BaseElement).Query(),
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
