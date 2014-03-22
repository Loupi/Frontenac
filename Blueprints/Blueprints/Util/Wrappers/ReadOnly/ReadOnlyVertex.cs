using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public ReadOnlyVertex(ReadOnlyGraph graph, IVertex baseVertex)
            : base(graph, baseVertex)
        {
            Contract.Requires(graph != null);
            Contract.Requires(baseVertex != null);

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable(ReadOnlyGraph, ((IVertex) BaseElement).GetEdges(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable(ReadOnlyGraph, ((IVertex) BaseElement).GetVertices(direction, labels));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new ReadOnlyEdgeIterable(ReadOnlyGraph, t.Edges()),
                                          t => new ReadOnlyVertexIterable(ReadOnlyGraph, t.Vertices()));
        }
    }
}