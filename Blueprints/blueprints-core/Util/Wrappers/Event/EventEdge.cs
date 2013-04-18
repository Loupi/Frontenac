using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An edge with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the edge.
    /// </summary>
    public class EventEdge : EventElement, Edge
    {
        public EventEdge(Edge baseEdge, EventGraph eventGraph)
            : base(baseEdge, eventGraph)
        {
        }

        public Vertex getVertex(Direction direction)
        {
            return new EventVertex(this.getBaseEdge().getVertex(direction), eventGraph);
        }

        public string getLabel()
        {
            return ((Edge)baseElement).getLabel();
        }

        public Edge getBaseEdge()
        {
            return (Edge)baseElement;
        }
    }
}
