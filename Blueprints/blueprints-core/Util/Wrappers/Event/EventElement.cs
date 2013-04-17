using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An element with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    /// the properties of the element.
    /// </summary>
    public abstract class EventElement : Element
    {
        protected readonly EventGraph _EventGraph;
        protected readonly Element _BaseElement;

        protected EventElement(Element baseElement, EventGraph eventGraph)
        {
            _BaseElement = baseElement;
            _EventGraph = eventGraph;
        }

        protected void OnVertexPropertyChanged(Vertex vertex, string key, object oldValue, object newValue)
        {
            _EventGraph.GetTrigger().AddEvent(new VertexPropertyChangedEvent(vertex, key, oldValue, newValue));
        }

        protected void OnEdgePropertyChanged(Edge edge, string key, object oldValue, object newValue)
        {
            _EventGraph.GetTrigger().AddEvent(new EdgePropertyChangedEvent(edge, key, oldValue, newValue));
        }

        protected void OnVertexPropertyRemoved(Vertex vertex, string key, object removedValue)
        {
            _EventGraph.GetTrigger().AddEvent(new VertexPropertyRemovedEvent(vertex, key, removedValue));
        }

        protected void OnEdgePropertyRemoved(Edge edge, string key, object removedValue)
        {
            _EventGraph.GetTrigger().AddEvent(new EdgePropertyRemovedEvent(edge, key, removedValue));
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return _BaseElement.GetPropertyKeys();
        }

        public object GetId()
        {
            return _BaseElement.GetId();
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyRemoved event.
        /// </note>
        public object RemoveProperty(string key)
        {
            object propertyRemoved = _BaseElement.RemoveProperty(key);

            if (this is Vertex)
                this.OnVertexPropertyRemoved((Vertex)this, key, propertyRemoved);
            else if (this is Edge)
                this.OnEdgePropertyRemoved((Edge)this, key, propertyRemoved);

            return propertyRemoved;
        }

        public object GetProperty(string key)
        {
            return _BaseElement.GetProperty(key);
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyChanged event.
        /// </note>
        public void SetProperty(string key, object value)
        {
            object oldValue = _BaseElement.GetProperty(key);
            _BaseElement.SetProperty(key, value);

            if (this is Vertex)
                this.OnVertexPropertyChanged((Vertex)this, key, oldValue, value);
            else if (this is Edge)
                this.OnEdgePropertyChanged((Edge)this, key, oldValue, value);
        }

        public override string ToString()
        {
            return _BaseElement.ToString();
        }

        public override int GetHashCode()
        {
            return _BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public Element GetBaseElement()
        {
            return _BaseElement;
        }

        public void Remove()
        {
            if (this is Vertex)
                _EventGraph.RemoveVertex((Vertex)this);
            else
                _EventGraph.RemoveEdge((Edge)this);
        }
    }
}
