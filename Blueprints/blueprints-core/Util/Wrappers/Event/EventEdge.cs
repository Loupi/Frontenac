namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An edge with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the edge.
    /// </summary>
    public class EventEdge : EventElement, IEdge
    {
        public EventEdge(IEdge baseEdge, EventGraph eventGraph)
            : base(baseEdge, eventGraph)
        {
        }

        public IVertex GetVertex(Direction direction)
        {
            return new EventVertex(GetBaseEdge().GetVertex(direction), EventGraph);
        }

        public string Label
        {
            get { return ((IEdge)BaseElement).Label; }
        }

        public IEdge GetBaseEdge()
        {
            return (IEdge)BaseElement;
        }
    }
}
