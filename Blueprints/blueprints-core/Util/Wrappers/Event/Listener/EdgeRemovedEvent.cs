using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeRemovedEvent : Event
    {
        readonly Edge _edge;

        public EdgeRemovedEvent(Edge edge)
        {
            _edge = edge;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.edgeRemoved(_edge);
            }
        }
    }
}
