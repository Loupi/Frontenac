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
        protected readonly Index _RawIndex;
        readonly EventGraph _EventGraph;

        public EventIndex(Index rawIndex, EventGraph eventGraph)
        {
            _RawIndex = rawIndex;
            _EventGraph = eventGraph;
        }

        public void Remove(string key, object value, Element element)
        {
            _RawIndex.Remove(key, value, (element as EventElement).GetBaseElement());
        }

        public void Put(string key, object value, Element element)
        {
            _RawIndex.Put(key, value, (element as EventElement).GetBaseElement());
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new EventVertexIterable((IEnumerable<Vertex>)_RawIndex.Get(key, value), _EventGraph);
            else
                return (CloseableIterable<Element>)new EventEdgeIterable((IEnumerable<Edge>)_RawIndex.Get(key, value), _EventGraph);
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new EventVertexIterable((IEnumerable<Vertex>)_RawIndex.Query(key, value), _EventGraph);
            else
                return (CloseableIterable<Element>)new EventEdgeIterable((IEnumerable<Edge>)_RawIndex.Query(key, value), _EventGraph);
        }

        public long Count(string key, object value)
        {
            return _RawIndex.Count(key, value);
        }

        public string GetIndexName()
        {
            return _RawIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return _RawIndex.GetIndexClass();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
