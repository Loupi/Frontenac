using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Base class for property changed events. 
    /// </summary>
    [ContractClass(typeof(VertexPropertyEventContract))]
    public abstract class VertexPropertyEvent : IEvent
    {
        readonly IVertex _vertex;
        readonly string _key;
        readonly object _oldValue;
        readonly object _newValue;

        protected VertexPropertyEvent(IVertex vertex, string key, object oldValue, object newValue)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            _vertex = vertex;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        protected abstract void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue, object newValue);

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _vertex, _key, _oldValue, _newValue);
            }
        }
    }
}
