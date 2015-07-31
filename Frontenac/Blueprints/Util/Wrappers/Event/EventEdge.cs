using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An edge with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the edge.
    /// </summary>
    public class EventEdge : EventElement, IEdge
    {
        private readonly IEdge _edge;

        public EventEdge(IEdge edge, EventGraph eventInnerTinkerGrapĥ)
            : base(edge, eventInnerTinkerGrapĥ)
        {
            Contract.Requires(edge != null);
            Contract.Requires(eventInnerTinkerGrapĥ != null);

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new EventVertex(GetBaseEdge().GetVertex(direction), EventInnerTinkerGrapĥ);
        }

        public string Label
        {
            get { return _edge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return _edge;
        }
    }
}