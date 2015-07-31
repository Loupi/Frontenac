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
        public EventVertex(IVertex vertex, EventGraph eventInnerTinkerGrapĥ)
            : base(vertex, eventInnerTinkerGrapĥ)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(eventInnerTinkerGrapĥ != null);

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new EventEdgeIterable(Vertex.GetEdges(direction, labels), EventInnerTinkerGrapĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new EventVertexIterable(Vertex.GetVertices(direction, labels), EventInnerTinkerGrapĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new EventEdgeIterable(t.Edges(), EventInnerTinkerGrapĥ),
                                          t => new EventVertexIterable(t.Vertices(), EventInnerTinkerGrapĥ));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return EventInnerTinkerGrapĥ.AddEdge(id, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}