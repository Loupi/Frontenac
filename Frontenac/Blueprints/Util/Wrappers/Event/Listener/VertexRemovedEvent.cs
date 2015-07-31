using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when a vertex is removed.
    /// </summary>
    public class VertexRemovedEvent : IEvent
    {
        private readonly IDictionary<string, object> _props;
        private readonly IVertex _vertex;

        public VertexRemovedEvent(IVertex vertex, IDictionary<string, object> props)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(props != null);

            _vertex = vertex;
            _props = props;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.VertexRemoved(_vertex, _props);
            }
        }
    }
}