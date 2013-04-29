using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeAddedEvent : IEvent
    {
        readonly IEdge _edge;

        public EdgeAddedEvent(IEdge edge)
        {
            _edge = edge;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeAdded(_edge);
            }
        }
    }
}
