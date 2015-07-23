using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public IdVertex(IVertex baseVertex, IdGraph idInnerTinkerGrapĥ)
            : base(baseVertex, idInnerTinkerGrapĥ, idInnerTinkerGrapĥ.GetSupportVertexIds())
        {
            Contract.Requires(baseVertex != null);
            Contract.Requires(idInnerTinkerGrapĥ != null);

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(_baseVertex.GetEdges(direction, labels), IdInnerTinkerGrapĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable(_baseVertex.GetVertices(direction, labels), IdInnerTinkerGrapĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new IdEdgeIterable(t.Edges(), IdInnerTinkerGrapĥ),
                                          t => new IdVertexIterable(t.Vertices(), IdInnerTinkerGrapĥ));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return IdInnerTinkerGrapĥ.AddEdge(id, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return _baseVertex;
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}