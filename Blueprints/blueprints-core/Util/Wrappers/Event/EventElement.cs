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
        protected readonly EventGraph eventGraph;
        protected readonly Element baseElement;

        protected EventElement(Element baseElement, EventGraph eventGraph)
        {
            this.baseElement = baseElement;
            this.eventGraph = eventGraph;
        }

        protected void onVertexPropertyChanged(Vertex vertex, string key, object oldValue, object newValue)
        {
            eventGraph.getTrigger().addEvent(new VertexPropertyChangedEvent(vertex, key, oldValue, newValue));
        }

        protected void onEdgePropertyChanged(Edge edge, string key, object oldValue, object newValue)
        {
            eventGraph.getTrigger().addEvent(new EdgePropertyChangedEvent(edge, key, oldValue, newValue));
        }

        protected void onVertexPropertyRemoved(Vertex vertex, string key, object removedValue)
        {
            eventGraph.getTrigger().addEvent(new VertexPropertyRemovedEvent(vertex, key, removedValue));
        }

        protected void onEdgePropertyRemoved(Edge edge, string key, object removedValue)
        {
            eventGraph.getTrigger().addEvent(new EdgePropertyRemovedEvent(edge, key, removedValue));
        }

        public IEnumerable<string> getPropertyKeys()
        {
            return baseElement.getPropertyKeys();
        }

        public object getId()
        {
            return baseElement.getId();
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyRemoved event.
        /// </note>
        public object removeProperty(string key)
        {
            object propertyRemoved = baseElement.removeProperty(key);

            if (this is Vertex)
                this.onVertexPropertyRemoved((Vertex)this, key, propertyRemoved);
            else if (this is Edge)
                this.onEdgePropertyRemoved((Edge)this, key, propertyRemoved);

            return propertyRemoved;
        }

        public object getProperty(string key)
        {
            return baseElement.getProperty(key);
        }

        /// <note>
        /// Raises a vertexPropertyRemoved or edgePropertyChanged event.
        /// </note>
        public void setProperty(string key, object value)
        {
            object oldValue = baseElement.getProperty(key);
            baseElement.setProperty(key, value);

            if (this is Vertex)
                this.onVertexPropertyChanged((Vertex)this, key, oldValue, value);
            else if (this is Edge)
                this.onEdgePropertyChanged((Edge)this, key, oldValue, value);
        }

        public override string ToString()
        {
            return baseElement.ToString();
        }

        public override int GetHashCode()
        {
            return baseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }

        public Element getBaseElement()
        {
            return baseElement;
        }

        public void remove()
        {
            if (this is Vertex)
                eventGraph.removeVertex((Vertex)this);
            else
                eventGraph.removeEdge((Edge)this);
        }
    }
}
