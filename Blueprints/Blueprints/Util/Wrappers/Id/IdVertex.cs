using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public IdVertex(IVertex baseVertex, IdGraph idInnerTinkerGraĥ)
            : base(baseVertex, idInnerTinkerGraĥ, idInnerTinkerGraĥ.GetSupportVertexIds())
        {
            Contract.Requires(baseVertex != null);
            Contract.Requires(idInnerTinkerGraĥ != null);

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(_baseVertex.GetEdges(direction, labels), IdInnerTinkerGraĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable(_baseVertex.GetVertices(direction, labels), IdInnerTinkerGraĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new IdEdgeIterable(t.Edges(), IdInnerTinkerGraĥ),
                                          t => new IdVertexIterable(t.Vertices(), IdInnerTinkerGraĥ));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return IdInnerTinkerGraĥ.AddEdge(null, this, vertex, label);
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