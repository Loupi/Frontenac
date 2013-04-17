using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public abstract class EdgePropertyEvent : Event
    {
        readonly Edge _Edge;
        readonly string _Key;
        readonly object _OldValue;
        readonly object _NewValue;

        public EdgePropertyEvent(Edge edge, string key, object oldValue, object newValue)
        {
            _Edge = edge;
            _Key = key;
            _OldValue = oldValue;
            _NewValue = newValue;
        }

        protected abstract void Fire(GraphChangedListener listener, Edge edge, string key, object oldValue, object newValue);

        public void FireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _Edge, _Key, _OldValue, _NewValue);
            }
        }
    }
}
