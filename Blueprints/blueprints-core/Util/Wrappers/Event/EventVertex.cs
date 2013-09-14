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
        private readonly IVertex _baseVertex;

        public EventVertex(IVertex baseVertex, EventGraph eventGraph)
            : base(baseVertex, eventGraph)
        {
            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return _baseVertex != null
                       ? new EventEdgeIterable(_baseVertex.GetEdges(direction, labels), EventGraph)
                       : null;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return _baseVertex != null
                       ? new EventVertexIterable(_baseVertex.GetVertices(direction, labels), EventGraph)
                       : null;
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new EventEdgeIterable(t.Edges(), EventGraph),
                                          t => new EventVertexIterable(t.Vertices(), EventGraph));
        }

        public IEdge AddEdge(string label, IVertex vertex)
        {
            return EventGraph.AddEdge(null, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);

            return _baseVertex;
        }
    }
}