using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An vertex with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the vertex.
    /// </summary>
    public class EventVertex : EventElement, IVertex
    {
        public EventVertex(IVertex vertex, EventGraph eventGraph)
            : base(vertex, eventGraph)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (eventGraph == null)
                throw new ArgumentNullException(nameof(eventGraph));

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new EventEdgeIterable(Vertex.GetEdges(direction, labels), EventGraph);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new EventVertexIterable(Vertex.GetVertices(direction, labels), EventGraph);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return Vertex.GetVertices(direction, label, ids);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return Vertex.GetNbEdges(direction, label);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new EventEdgeIterable(t.Edges(), EventGraph),
                                          t => new EventVertexIterable(t.Vertices(), EventGraph));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return EventGraph.AddEdge(id, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}