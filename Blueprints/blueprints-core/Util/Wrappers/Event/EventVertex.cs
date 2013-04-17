using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An vertex with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the vertex.
    /// </summary>
    public class EventVertex : EventElement, Vertex
    {
        public EventVertex(Vertex baseVertex, EventGraph eventGraph)
            : base(baseVertex, eventGraph)
        {

        }

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            return new EventEdgeIterable((_BaseElement as Vertex).GetEdges(direction, labels), _EventGraph);
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new EventVertexIterable((_BaseElement as Vertex).GetVertices(direction, labels), _EventGraph);
        }

        public VertexQuery Query()
        {
            return new WrapperVertexQuery((_BaseElement as Vertex).Query(),
                t => new EventEdgeIterable(t.Edges(), _EventGraph),
                t => new EventVertexIterable(t.Vertices(), _EventGraph));
        }

        public Edge AddEdge(string label, Vertex vertex)
        {
            return _EventGraph.AddEdge(null, this, vertex, label);
        }

        public Vertex GetBaseVertex()
        {
            return this._BaseElement as Vertex;
        }
    }
}
