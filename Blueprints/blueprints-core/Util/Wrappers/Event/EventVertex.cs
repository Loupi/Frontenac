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

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            return new EventEdgeIterable((baseElement as Vertex).getEdges(direction, labels), eventGraph);
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new EventVertexIterable((baseElement as Vertex).getVertices(direction, labels), eventGraph);
        }

        public VertexQuery query()
        {
            return new WrapperVertexQuery((baseElement as Vertex).query(),
                t => new EventEdgeIterable(t.edges(), eventGraph),
                t => new EventVertexIterable(t.vertices(), eventGraph));
        }

        public Edge addEdge(string label, Vertex vertex)
        {
            return eventGraph.addEdge(null, this, vertex, label);
        }

        public Vertex getBaseVertex()
        {
            return this.baseElement as Vertex;
        }
    }
}
