using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An edge with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the edge.
    /// </summary>
    public class EventEdge : EventElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public EventEdge(IEdge baseEdge, EventGraph eventGraph)
            : base(baseEdge, eventGraph)
        {
            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new EventVertex(GetBaseEdge().GetVertex(direction), EventGraph);
        }

        public string Label
        {
            get { return _baseEdge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return _baseEdge;
        }
    }
}