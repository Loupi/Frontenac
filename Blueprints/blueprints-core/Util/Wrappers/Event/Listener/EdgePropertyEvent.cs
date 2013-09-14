using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    [ContractClass(typeof (EdgePropertyEventContract))]
    public abstract class EdgePropertyEvent : IEvent
    {
        private readonly IEdge _edge;
        private readonly string _key;
        private readonly object _newValue;
        private readonly object _oldValue;

        protected EdgePropertyEvent(IEdge edge, string key, object oldValue, object newValue)
        {
            Contract.Requires(edge != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            _edge = edge;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _edge, _key, _oldValue, _newValue);
            }
        }

        protected abstract void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue);
    }
}