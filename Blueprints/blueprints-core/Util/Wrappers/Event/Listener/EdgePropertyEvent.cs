using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public abstract class EdgePropertyEvent : IEvent
    {
        readonly IEdge _edge;
        readonly string _key;
        readonly object _oldValue;
        readonly object _newValue;

        protected EdgePropertyEvent(IEdge edge, string key, object oldValue, object newValue)
        {
            _edge = edge;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        protected abstract void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue, object newValue);

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _edge, _key, _oldValue, _newValue);
            }
        }
    }
}
