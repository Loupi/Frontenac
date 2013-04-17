using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeAddedEvent : Event
    {
        readonly Edge _Edge;

        public EdgeAddedEvent(Edge edge)
        {
            _Edge = edge;
        }

        public void FireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeAdded(_Edge);
            }
        }
    }
}
