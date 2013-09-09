using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, IVertex
    {
        readonly IVertex _baseVertex;

        public ReadOnlyVertex(IVertex baseVertex)
            : base(baseVertex)
        {
            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable(((IVertex) BaseElement).GetEdges(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable(((IVertex) BaseElement).GetVertices(direction, labels));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                t => new ReadOnlyEdgeIterable(t.Edges()),
                t => new ReadOnlyVertexIterable(t.Vertices()));
        }
    }
}
