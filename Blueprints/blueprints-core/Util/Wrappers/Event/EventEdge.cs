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

        public Vertex GetVertex(Direction direction)
        {
            return new EventVertex(this.GetBaseEdge().GetVertex(direction), _EventGraph);
        }

        public string GetLabel()
        {
            return ((Edge)_BaseElement).GetLabel();
        }

        public Edge GetBaseEdge()
        {
            return (Edge)_BaseElement;
        }
    }
}
