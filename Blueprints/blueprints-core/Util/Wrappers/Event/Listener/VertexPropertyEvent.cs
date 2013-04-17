using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public abstract class VertexPropertyEvent : Event
    {
        readonly Vertex _Vertex;
        readonly string _Key;
        readonly object _OldValue;
        readonly object _NewValue;

        public VertexPropertyEvent(Vertex vertex, string key, object oldValue, object newValue)
        {
            _Vertex = vertex;
            _Key = key;
            _OldValue = oldValue;
            _NewValue = newValue;
        }

        protected abstract void Fire(GraphChangedListener listener, Vertex vertex, string key, object oldValue, object newValue);

        public void FireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _Vertex, _Key, _OldValue, _NewValue);
            }
        }
    }
}
