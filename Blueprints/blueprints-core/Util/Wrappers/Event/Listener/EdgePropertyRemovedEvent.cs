namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event fired when an edge property is removed. 
    /// </summary>
    public class EdgePropertyRemovedEvent : EdgePropertyEvent
    {
        public EdgePropertyRemovedEvent(IEdge vertex, string key, object oldValue)
            : base(vertex, key, oldValue, null)
        {

        }

        protected override void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue, object newValue)
        {
            listener.EdgePropertyRemoved(edge, key, oldValue);
        }
    }
}
