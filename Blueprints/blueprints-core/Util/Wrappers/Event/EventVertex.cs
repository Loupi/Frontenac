using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An vertex with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the vertex.
    /// </summary>
    public class EventVertex : EventElement, IVertex
    {
        public EventVertex(IVertex baseVertex, EventGraph eventGraph)
            : base(baseVertex, eventGraph)
        {

        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new EventEdgeIterable(vertex.GetEdges(direction, labels), EventGraph);
            return null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new EventVertexIterable(vertex.GetVertices(direction, labels), EventGraph);
            return null;
        }

        public IVertexQuery Query()
        {
            var vertex = BaseElement as IVertex;
            if (vertex != null)
                return new WrapperVertexQuery(vertex.Query(),
                                              t => new EventEdgeIterable(t.Edges(), EventGraph),
                                              t => new EventVertexIterable(t.Vertices(), EventGraph));
            return null;
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return EventGraph.AddEdge(null, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            return BaseElement as IVertex;
        }
    }
}
