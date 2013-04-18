using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public abstract class EdgePropertyEvent : Event
    {
        readonly Edge _edge;
        readonly string _key;
        readonly object _oldValue;
        readonly object _newValue;

        protected EdgePropertyEvent(Edge edge, string key, object oldValue, object newValue)
        {
            _edge = edge;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        protected abstract void fire(GraphChangedListener listener, Edge edge, string key, object oldValue, object newValue);

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                fire(eventListeners.Current, _edge, _key, _oldValue, _newValue);
            }
        }
    }
}
