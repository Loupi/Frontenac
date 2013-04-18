using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event fired when an edge is removed. 
    /// </summary>
    public class EdgeRemovedEvent : Event
    {
        readonly Edge _edge;
        readonly IDictionary<string, object> _props;

        public EdgeRemovedEvent(Edge edge, IDictionary<string, object> props)
        {
            _edge = edge;
            _props = props;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.edgeRemoved(_edge, _props);
            }
        }
    }
}
