using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An index that wraps graph elements in the "evented" way. This class does not directly raise graph events, but
    /// passes the GraphChangedListener to the edges and vertices returned from indices so that they may raise graph
    /// events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventIndex : Index
    {
        protected readonly Index rawIndex;
        readonly EventGraph _eventGraph;

        public EventIndex(Index rawIndex, EventGraph eventGraph)
        {
            this.rawIndex = rawIndex;
            _eventGraph = eventGraph;
        }

        public void remove(string key, object value, Element element)
        {
            rawIndex.remove(key, value, (element as EventElement).getBaseElement());
        }

        public void put(string key, object value, Element element)
        {
            rawIndex.put(key, value, (element as EventElement).getBaseElement());
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new EventVertexIterable((IEnumerable<Vertex>)rawIndex.get(key, value), _eventGraph);
            else
                return (CloseableIterable<Element>)new EventEdgeIterable((IEnumerable<Edge>)rawIndex.get(key, value), _eventGraph);
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new EventVertexIterable((IEnumerable<Vertex>)rawIndex.query(key, value), _eventGraph);
            else
                return (CloseableIterable<Element>)new EventEdgeIterable((IEnumerable<Edge>)rawIndex.query(key, value), _eventGraph);
        }

        public long count(string key, object value)
        {
            return rawIndex.count(key, value);
        }

        public string getIndexName()
        {
            return rawIndex.getIndexName();
        }

        public Type getIndexClass()
        {
            return rawIndex.getIndexClass();
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
