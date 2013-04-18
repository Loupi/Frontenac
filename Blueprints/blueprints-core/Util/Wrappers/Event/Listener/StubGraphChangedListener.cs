using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class StubGraphChangedListener : GraphChangedListener
    {
        int _addEdgeEvent = 0;
        int _addVertexEvent = 0;
        int _vertexPropertyChangedEvent = 0;
        int _vertexPropertyRemovedEvent = 0;
        int _vertexRemovedEvent = 0;
        int _edgePropertyChangedEvent = 0;
        int _edgePropertyRemovedEvent = 0;
        int _edgeRemovedEvent = 0;

        readonly List<string> _order = new List<string>();

        public void reset()
        {
            _addEdgeEvent = 0;
            _addVertexEvent = 0;
            _vertexPropertyChangedEvent = 0;
            _vertexPropertyRemovedEvent = 0;
            _vertexRemovedEvent = 0;
            _edgePropertyChangedEvent = 0;
            _edgePropertyRemovedEvent = 0;
            _edgeRemovedEvent = 0;

            _order.Clear();
        }

        public List<string> getOrder()
        {
            return this._order;
        }

        public void vertexAdded(Vertex vertex)
        {
            _addVertexEvent++;
            _order.Add(string.Concat("v-added-", vertex.getId()));
        }

        public void vertexPropertyChanged(Vertex vertex, string s, object o, object n)
        {
            _vertexPropertyChangedEvent++;
            _order.Add(string.Concat("v-property-changed-", vertex.getId(), "-", s, ":", o, "->", n));
        }

        public void vertexPropertyRemoved(Vertex vertex, string s, object o)
        {
            _vertexPropertyRemovedEvent++;
            _order.Add(string.Concat("v-property-removed-", vertex.getId(), "-", s, ":", o));
        }

        public void vertexRemoved(Vertex vertex)
        {
            _vertexRemovedEvent++;
            _order.Add(string.Concat("v-removed-", vertex.getId()));
        }

        public void edgeAdded(Edge edge)
        {
            _addEdgeEvent++;
            _order.Add(string.Concat("e-added-", edge.getId()));
        }

        public void edgePropertyChanged(Edge edge, string s, object o, object n)
        {
            _edgePropertyChangedEvent++;
            _order.Add(string.Concat("e-property-changed-", edge.getId(), "-", s, ":", o, "->", n));
        }

        public void edgePropertyRemoved(Edge edge, string s, object o)
        {
            _edgePropertyRemovedEvent++;
            _order.Add(string.Concat("e-property-removed-", edge.getId(), "-", s, ":", o));
        }

        public void edgeRemoved(Edge edge)
        {
            _edgeRemovedEvent++;
            _order.Add(string.Concat("e-removed-", edge.getId()));
        }

        public int addEdgeEventRecorded()
        {
            return _addEdgeEvent;
        }

        public int addVertexEventRecorded()
        {
            return _addVertexEvent;
        }

        public int vertexPropertyChangedEventRecorded()
        {
            return _vertexPropertyChangedEvent;
        }

        public int vertexPropertyRemovedEventRecorded()
        {
            return _vertexPropertyRemovedEvent;
        }

        public int vertexRemovedEventRecorded()
        {
            return _vertexRemovedEvent;
        }

        public int edgePropertyChangedEventRecorded()
        {
            return _edgePropertyChangedEvent;
        }

        public int edgePropertyRemovedEventRecorded()
        {
            return _edgePropertyRemovedEvent;
        }

        public int edgeRemovedEventRecorded()
        {
            return _edgeRemovedEvent;
        }
    }
}
