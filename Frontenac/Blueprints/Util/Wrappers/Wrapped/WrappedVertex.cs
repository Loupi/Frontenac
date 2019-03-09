using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedVertex : WrappedElement, IVertex
    {
        public WrappedVertex(IVertex vertex)
            : base(vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return Vertex.GetEdges(direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return Vertex.GetVertices(direction, label, ids);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return Vertex.GetVertices(direction, labels);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return Vertex.GetNbEdges(direction, label);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(), t => t.Edges(), t => t.Vertices());
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            var wrappedVertex = vertex as WrappedVertex;
            return wrappedVertex != null 
                ? new WrappedEdge(Vertex.AddEdge(id, label, wrappedVertex.Vertex)) 
                : new WrappedEdge(Vertex.AddEdge(id, label, vertex));
        }

        public IVertex Vertex { get; protected set; }
    }
}