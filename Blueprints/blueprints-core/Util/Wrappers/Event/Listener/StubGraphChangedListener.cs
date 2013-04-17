using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class StubGraphChangedListener : GraphChangedListener
    {
        int _AddEdgeEvent = 0;
        int _AddVertexEvent = 0;
        int _VertexPropertyChangedEvent = 0;
        int _VertexPropertyRemovedEvent = 0;
        int _VertexRemovedEvent = 0;
        int _EdgePropertyChangedEvent = 0;
        int _EdgePropertyRemovedEvent = 0;
        int _EdgeRemovedEvent = 0;

        readonly List<string> _Order = new List<string>();

        public void Reset()
        {
            _AddEdgeEvent = 0;
            _AddVertexEvent = 0;
            _VertexPropertyChangedEvent = 0;
            _VertexPropertyRemovedEvent = 0;
            _VertexRemovedEvent = 0;
            _EdgePropertyChangedEvent = 0;
            _EdgePropertyRemovedEvent = 0;
            _EdgeRemovedEvent = 0;

            _Order.Clear();
        }

        public List<string> GetOrder()
        {
            return this._Order;
        }

        public void VertexAdded(Vertex vertex)
        {
            _AddVertexEvent++;
            _Order.Add(string.Concat("v-added-", vertex.GetId()));
        }

        public void VertexPropertyChanged(Vertex vertex, string s, object o, object n)
        {
            _VertexPropertyChangedEvent++;
            _Order.Add(string.Concat("v-property-changed-", vertex.GetId(), "-", s, ":", o, "->", n));
        }

        public void VertexPropertyRemoved(Vertex vertex, string s, object o)
        {
            _VertexPropertyRemovedEvent++;
            _Order.Add(string.Concat("v-property-removed-", vertex.GetId(), "-", s, ":", o));
        }

        public void VertexRemoved(Vertex vertex)
        {
            _VertexRemovedEvent++;
            _Order.Add(string.Concat("v-removed-", vertex.GetId()));
        }

        public void EdgeAdded(Edge edge)
        {
            _AddEdgeEvent++;
            _Order.Add(string.Concat("e-added-", edge.GetId()));
        }

        public void EdgePropertyChanged(Edge edge, string s, object o, object n)
        {
            _EdgePropertyChangedEvent++;
            _Order.Add(string.Concat("e-property-changed-", edge.GetId(), "-", s, ":", o, "->", n));
        }

        public void EdgePropertyRemoved(Edge edge, string s, object o)
        {
            _EdgePropertyRemovedEvent++;
            _Order.Add(string.Concat("e-property-removed-", edge.GetId(), "-", s, ":", o));
        }

        public void EdgeRemoved(Edge edge)
        {
            _EdgeRemovedEvent++;
            _Order.Add(string.Concat("e-removed-", edge.GetId()));
        }

        public int AddEdgeEventRecorded()
        {
            return _AddEdgeEvent;
        }

        public int AddVertexEventRecorded()
        {
            return _AddVertexEvent;
        }

        public int VertexPropertyChangedEventRecorded()
        {
            return _VertexPropertyChangedEvent;
        }

        public int VertexPropertyRemovedEventRecorded()
        {
            return _VertexPropertyRemovedEvent;
        }

        public int VertexRemovedEventRecorded()
        {
            return _VertexRemovedEvent;
        }

        public int EdgePropertyChangedEventRecorded()
        {
            return _EdgePropertyChangedEvent;
        }

        public int edgePropertyRemovedEventRecorded()
        {
            return _EdgePropertyRemovedEvent;
        }

        public int edgeRemovedEventRecorded()
        {
            return _EdgeRemovedEvent;
        }
    }
}
