using System.Collections.Generic;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An element with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the element.
    /// </summary>
    public abstract class EventElement : IElement
    {
        protected readonly EventGraph EventGraph;
        protected readonly IElement BaseElement;

        protected EventElement(IElement baseElement, EventGraph eventGraph)
        {
            BaseElement = baseElement;
            EventGraph = eventGraph;
        }

        protected void OnVertexPropertyChanged(IVertex vertex, string key, object oldValue, object newValue)
        {
            EventGraph.GetTrigger().AddEvent(new VertexPropertyChangedEvent(vertex, key, oldValue, newValue));
        }

        protected void OnEdgePropertyChanged(IEdge edge, string key, object oldValue, object newValue)
        {
            EventGraph.GetTrigger().AddEvent(new EdgePropertyChangedEvent(edge, key, oldValue, newValue));
        }

        protected void OnVertexPropertyRemoved(IVertex vertex, string key, object removedValue)
        {
            EventGraph.GetTrigger().AddEvent(new VertexPropertyRemovedEvent(vertex, key, removedValue));
        }

        protected void OnEdgePropertyRemoved(IEdge edge, string key, object removedValue)
        {
            EventGraph.GetTrigger().AddEvent(new EdgePropertyRemovedEvent(edge, key, removedValue));
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public object Id
        {
            get { return BaseElement.Id; }
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyRemoved event.
        /// </note>
        public object RemoveProperty(string key)
        {
            object propertyRemoved = BaseElement.RemoveProperty(key);

            var vertex = this as IVertex;
            if (vertex != null)
                OnVertexPropertyRemoved(vertex, key, propertyRemoved);
            else
            {
                var edge = this as IEdge;
                if (edge != null)
                    OnEdgePropertyRemoved(edge, key, propertyRemoved);
            }

            return propertyRemoved;
        }

        public object GetProperty(string key)
        {
            return BaseElement.GetProperty(key);
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyChanged event.
        /// </note>
        public void SetProperty(string key, object value)
        {
            object oldValue = BaseElement.GetProperty(key);
            BaseElement.SetProperty(key, value);

            var vertex = this as IVertex;
            if (vertex != null)
                OnVertexPropertyChanged(vertex, key, oldValue, value);
            else
            {
                var edge = this as IEdge;
                if (edge != null)
                    OnEdgePropertyChanged(edge, key, oldValue, value);
            }
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public IElement GetBaseElement()
        {
            return BaseElement;
        }

        public void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                EventGraph.RemoveVertex(vertex);
            else
                EventGraph.RemoveEdge((IEdge)this);
        }
    }
}
