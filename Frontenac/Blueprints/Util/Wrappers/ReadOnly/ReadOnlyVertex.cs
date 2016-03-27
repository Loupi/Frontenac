using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public ReadOnlyVertex(ReadOnlyGraph innerTinkerGrapĥ, IVertex baseVertex)
            : base(innerTinkerGrapĥ, baseVertex)
        {
            Contract.Requires(innerTinkerGrapĥ != null);
            Contract.Requires(baseVertex != null);

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGrapĥ, ((IVertex) BaseElement).GetEdges(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGrapĥ, ((IVertex) BaseElement).GetVertices(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return _baseVertex.GetVertices(direction, label, ids);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return _baseVertex.GetNbEdges(direction, label);
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGrapĥ, t.Edges()),
                                          t => new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGrapĥ, t.Vertices()));
        }
    }
}