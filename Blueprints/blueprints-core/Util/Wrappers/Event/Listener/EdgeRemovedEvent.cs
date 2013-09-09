using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event fired when an edge is removed. 
    /// </summary>
    public class EdgeRemovedEvent : IEvent
    {
        readonly IEdge _edge;
        readonly IDictionary<string, object> _props;

        public EdgeRemovedEvent(IEdge edge, IDictionary<string, object> props)
        {
            Contract.Requires(edge != null);
            Contract.Requires(props != null);

            _edge = edge;
            _props = props;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeRemoved(_edge, _props);
            }
        }
    }
}
