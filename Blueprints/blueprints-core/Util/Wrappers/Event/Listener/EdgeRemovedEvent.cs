using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeRemovedEvent : Event
    {
        readonly Edge _Edge;

        public EdgeRemovedEvent(Edge edge)
        {
            _Edge = edge;
        }

        public void FireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeRemoved(_Edge);
            }
        }
    }
}
