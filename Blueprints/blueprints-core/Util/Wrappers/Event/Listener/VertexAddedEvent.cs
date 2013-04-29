using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event that fires when a vertex is added to a graph.
    /// </summary>
    public class VertexAddedEvent : IEvent
    {
        readonly IVertex _vertex;

        public VertexAddedEvent(IVertex vertex)
        {
            _vertex = vertex;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.VertexAdded(_vertex);
            }
        }
    }
}
