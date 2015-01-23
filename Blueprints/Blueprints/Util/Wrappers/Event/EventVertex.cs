using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An vertex with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the vertex.
    /// </summary>
    public class EventVertex : EventElement, IVertex
    {
        public EventVertex(IVertex vertex, EventGraph eventInnerTinkerGraĥ)
            : base(vertex, eventInnerTinkerGraĥ)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(eventInnerTinkerGraĥ != null);

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new EventEdgeIterable(Vertex.GetEdges(direction, labels), EventInnerTinkerGraĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new EventVertexIterable(Vertex.GetVertices(direction, labels), EventInnerTinkerGraĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new EventEdgeIterable(t.Edges(), EventInnerTinkerGraĥ),
                                          t => new EventVertexIterable(t.Vertices(), EventInnerTinkerGraĥ));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return EventInnerTinkerGraĥ.AddEdge(null, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}