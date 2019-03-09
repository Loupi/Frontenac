using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public IdVertex(IVertex baseVertex, IdGraph idGraph)
            : base(baseVertex, idGraph, idGraph.GetSupportVertexIds())
        {
            if (baseVertex == null)
                throw new ArgumentNullException(nameof(baseVertex));
            if (idGraph == null)
                throw new ArgumentNullException(nameof(idGraph));

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(_baseVertex.GetEdges(direction, labels), IdGraph);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return _baseVertex.GetNbEdges(direction, label);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return _baseVertex.GetVertices(direction, label, ids);
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

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return IdGraph.AddEdge(id, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            return _baseVertex;
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}