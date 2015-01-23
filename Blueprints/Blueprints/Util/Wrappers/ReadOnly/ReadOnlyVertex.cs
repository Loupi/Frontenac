using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public ReadOnlyVertex(ReadOnlyGraph innerTinkerGraĥ, IVertex baseVertex)
            : base(innerTinkerGraĥ, baseVertex)
        {
            Contract.Requires(innerTinkerGraĥ != null);
            Contract.Requires(baseVertex != null);

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGraĥ, ((IVertex) BaseElement).GetEdges(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGraĥ, ((IVertex) BaseElement).GetVertices(direction, labels));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGraĥ, t.Edges()),
                                          t => new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGraĥ, t.Vertices()));
        }
    }
}