using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// An EventGraph is a wrapper to existing Graph implementations and provides for graph events to be raised
    /// to one or more listeners on changes to the Graph. Notifications to the listeners occur for the
    /// following events: new vertex/edge, vertex/edge property changed, vertex/edge property removed,
    /// vertex/edge removed.
    /// 
    /// The limiting factor to events being raised is related to out-of-process functions changing graph elements.
    /// 
    /// To gather events from EventGraph, simply provide an implementation of the {@link GraphChangedListener} to
    /// the EventGraph by utilizing the addListener method. EventGraph allows the addition of multiple GraphChangedListener
    /// implementations. Each listener will be notified in the order that it was added.
    /// </summary>
    public class EventGraph : Graph, WrapperGraph
    {
        protected EventTrigger _Trigger;
        protected Graph _BaseGraph;
        protected readonly List<GraphChangedListener> _GraphChangedListeners = new List<GraphChangedListener>();
        readonly Features _Features;

        public EventGraph(Graph baseGraph)
        {
            _BaseGraph = baseGraph;
            _Features = _BaseGraph.GetFeatures().CopyFeatures();
            _Features.IsWrapper = true;

            _Trigger = new EventTrigger(this, false);
        }

        public void RemoveAllListeners()
        {
            _GraphChangedListeners.Clear();
        }

        public void addListener(GraphChangedListener listener)
        {
            _GraphChangedListeners.Add(listener);
        }

        public IEnumerator<GraphChangedListener> GetListenerIterator()
        {
            return _GraphChangedListeners.GetEnumerator();
        }

        public EventTrigger GetTrigger()
        {
            return _Trigger;
        }

        public void RemoveListener(GraphChangedListener listener)
        {
            _GraphChangedListeners.Remove(listener);
        }

        protected void OnVertexAdded(Vertex vertex)
        {
            _Trigger.AddEvent(new VertexAddedEvent(vertex));
        }

        protected void OnVertexRemoved(Vertex vertex)
        {
            _Trigger.AddEvent(new VertexRemovedEvent(vertex));
        }

        protected void OnEdgeAdded(Edge edge)
        {
            _Trigger.AddEvent(new EdgeAddedEvent(edge));
        }

        protected void OnEdgeRemoved(Edge edge)
        {
            _Trigger.AddEvent(new EdgeRemovedEvent(edge));
        }

        /// <note>
        /// Raises a vertexAdded event.
        /// </note>
        public Vertex AddVertex(object id)
        {
            Vertex vertex = _BaseGraph.AddVertex(id);
            if (vertex == null)
                return null;
            else
            {
                this.OnVertexAdded(vertex);
                return new EventVertex(vertex, this);
            }
        }

        public Vertex GetVertex(object id)
        {
            Vertex vertex = _BaseGraph.GetVertex(id);
            if (null == vertex)
                return null;

            return new EventVertex(vertex, this);
        }

        /// <note>
        /// Raises a vertexRemoved event.
        /// </note>
        public void RemoveVertex(Vertex vertex)
        {
            Vertex vertexToRemove = vertex;
            if (vertex is EventVertex)
                vertexToRemove = (vertex as EventVertex).GetBaseVertex();

            this._BaseGraph.RemoveVertex(vertexToRemove);
            this.OnVertexRemoved(vertex);
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return new EventVertexIterable(_BaseGraph.GetVertices(), this);
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            return new EventVertexIterable(_BaseGraph.GetVertices(key, value), this);
        }

        /// <note>
        /// Raises an edgeAdded event.
        /// </note>
        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            Vertex outVertexToSet = outVertex;
            if (outVertex is EventVertex)
                outVertexToSet = (outVertex as EventVertex).GetBaseVertex();

            Vertex inVertexToSet = inVertex;
            if (inVertex is EventVertex)
                inVertexToSet = (inVertex as EventVertex).GetBaseVertex();

            Edge edge = _BaseGraph.AddEdge(id, outVertexToSet, inVertexToSet, label);
            if (edge == null)
                return null;
            else
            {
                this.OnEdgeAdded(edge);
                return new EventEdge(edge, this);
            }
        }

        public Edge GetEdge(object id)
        {
            Edge edge = _BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new EventEdge(edge, this);
        }

        /// <note>
        /// Raises an edgeRemoved event.
        /// </note>
        public void RemoveEdge(Edge edge)
        {
            Edge edgeToRemove = edge;
            if (edge is EventEdge)
                edgeToRemove = (edge as EventEdge).GetBaseEdge();

            _BaseGraph.RemoveEdge(edgeToRemove);
            this.OnEdgeRemoved(edge);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return new EventEdgeIterable(_BaseGraph.GetEdges(), this);
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return new EventEdgeIterable(_BaseGraph.GetEdges(key, value), this);
        }

        public GraphQuery Query()
        {
            return new WrappedGraphQuery(_BaseGraph.Query(),
                t => new EventEdgeIterable(t.Edges(), this),
                t => new EventVertexIterable(t.Vertices(), this));
        }

        public void Shutdown()
        {
            try
            {
                _BaseGraph.Shutdown();

                // TODO: hmmmmmm??
                _Trigger.FireEventQueue();
                _Trigger.ResetEventQueue();
            }
            catch (Exception)
            {

            }
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public Features GetFeatures()
        {
            return _Features;
        }
    }
}
