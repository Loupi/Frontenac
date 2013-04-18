using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeAddedEvent : Event
    {
        readonly Edge _edge;

        public EdgeAddedEvent(Edge edge)
        {
            _edge = edge;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.edgeAdded(_edge);
            }
        }
    }
}
