using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public abstract class VertexPropertyEvent : Event
    {
        readonly Vertex _vertex;
        readonly string _key;
        readonly object _oldValue;
        readonly object _newValue;

        protected VertexPropertyEvent(Vertex vertex, string key, object oldValue, object newValue)
        {
            _vertex = vertex;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        protected abstract void fire(GraphChangedListener listener, Vertex vertex, string key, object oldValue, object newValue);

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                fire(eventListeners.Current, _vertex, _key, _oldValue, _newValue);
            }
        }
    }
}
