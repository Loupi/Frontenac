namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when a vertex property is removed.
    /// </summary>
    public class VertexPropertyRemovedEvent : VertexPropertyEvent
    {
        public VertexPropertyRemovedEvent(IVertex vertex, string key, object removedValue)
            : base(vertex, key, removedValue, null)
        {
        }

        protected override void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue)
        {
            listener.VertexPropertyRemoved(vertex, key, oldValue);
        }
    }
}