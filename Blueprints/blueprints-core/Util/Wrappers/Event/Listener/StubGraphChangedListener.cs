using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// An event listener that acts as a counter for changes to the graph. 
    /// </summary>
    public class StubGraphChangedListener : GraphChangedListener
    {
        long _addEdgeEvent = 0;
        long _addVertexEvent = 0;
        long _vertexPropertyChangedEvent = 0;
        long _vertexPropertyRemovedEvent = 0;
        long _vertexRemovedEvent = 0;
        long _edgePropertyChangedEvent = 0;
        long _edgePropertyRemovedEvent = 0;
        long _edgeRemovedEvent = 0;

        ConcurrentQueue<string> _order = new ConcurrentQueue<string>();

        public void reset()
        {
            Interlocked.Exchange(ref _addEdgeEvent, 0);
            Interlocked.Exchange(ref _addVertexEvent, 0);
            Interlocked.Exchange(ref _vertexPropertyChangedEvent, 0);
            Interlocked.Exchange(ref _vertexPropertyRemovedEvent, 0);
            Interlocked.Exchange(ref _vertexRemovedEvent, 0);
            Interlocked.Exchange(ref _edgePropertyChangedEvent, 0);
            Interlocked.Exchange(ref _edgePropertyRemovedEvent, 0);
            Interlocked.Exchange(ref _edgeRemovedEvent, 0);

            _order = new ConcurrentQueue<string>();
        }

        public List<string> getOrder()
        {
            return this._order.ToList();
        }

        public void vertexAdded(Vertex vertex)
        {
            Interlocked.Increment(ref _addVertexEvent);
            _order.Enqueue(string.Concat("v-added-", vertex.getId()));
        }

        public void vertexPropertyChanged(Vertex vertex, string s, object o, object n)
        {
            Interlocked.Increment(ref _vertexPropertyChangedEvent);
            _order.Enqueue(string.Concat("v-property-changed-", vertex.getId(), "-", s, ":", o, "->", n));
        }

        public void vertexPropertyRemoved(Vertex vertex, string s, object o)
        {
            Interlocked.Increment(ref _vertexPropertyRemovedEvent);
            _order.Enqueue(string.Concat("v-property-removed-", vertex.getId(), "-", s, ":", o));
        }

        public void vertexRemoved(Vertex vertex, IDictionary<string, object> props)
        {
            Interlocked.Increment(ref _vertexRemovedEvent);
            _order.Enqueue(string.Concat("v-removed-", vertex.getId()));
        }

        public void edgeAdded(Edge edge)
        {
            Interlocked.Increment(ref _addEdgeEvent);
            _order.Enqueue(string.Concat("e-added-", edge.getId()));
        }

        public void edgePropertyChanged(Edge edge, string s, object o, object n)
        {
            Interlocked.Increment(ref _edgePropertyChangedEvent);
            _order.Enqueue(string.Concat("e-property-changed-", edge.getId(), "-", s, ":", o, "->", n));
        }

        public void edgePropertyRemoved(Edge edge, string s, object o)
        {
            Interlocked.Increment(ref _edgePropertyRemovedEvent);
            _order.Enqueue(string.Concat("e-property-removed-", edge.getId(), "-", s, ":", o));
        }

        public void edgeRemoved(Edge edge, IDictionary<string, object> props)
        {
            Interlocked.Increment(ref _edgeRemovedEvent);
            _order.Enqueue(string.Concat("e-removed-", edge.getId()));
        }

        public long addEdgeEventRecorded()
        {
            return Interlocked.Read(ref _addEdgeEvent);
        }

        public long addVertexEventRecorded()
        {
            return Interlocked.Read(ref _addVertexEvent);
        }

        public long vertexPropertyChangedEventRecorded()
        {
            return Interlocked.Read(ref _vertexPropertyChangedEvent);
        }

        public long vertexPropertyRemovedEventRecorded()
        {
            return Interlocked.Read(ref _vertexPropertyRemovedEvent);
        }

        public long vertexRemovedEventRecorded()
        {
            return Interlocked.Read(ref _vertexRemovedEvent);
        }

        public long edgePropertyChangedEventRecorded()
        {
            return Interlocked.Read(ref _edgePropertyChangedEvent);
        }

        public long edgePropertyRemovedEventRecorded()
        {
            return Interlocked.Read(ref _edgePropertyRemovedEvent);
        }

        public long edgeRemovedEventRecorded()
        {
            return Interlocked.Read(ref _edgeRemovedEvent);
        }
    }
}
