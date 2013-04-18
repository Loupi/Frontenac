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
        protected EventTrigger trigger;
        protected Graph baseGraph;
        protected readonly List<GraphChangedListener> graphChangedListeners = new List<GraphChangedListener>();
        readonly Features _features;

        public EventGraph(Graph baseGraph)
        {
            this.baseGraph = baseGraph;
            _features = this.baseGraph.getFeatures().copyFeatures();
            _features.isWrapper = true;

            trigger = new EventTrigger(this, false);
        }

        public void removeAllListeners()
        {
            graphChangedListeners.Clear();
        }

        public void addListener(GraphChangedListener listener)
        {
            graphChangedListeners.Add(listener);
        }

        public IEnumerator<GraphChangedListener> getListenerIterator()
        {
            return graphChangedListeners.GetEnumerator();
        }

        public EventTrigger getTrigger()
        {
            return trigger;
        }

        public void removeListener(GraphChangedListener listener)
        {
            graphChangedListeners.Remove(listener);
        }

        protected void onVertexAdded(Vertex vertex)
        {
            trigger.addEvent(new VertexAddedEvent(vertex));
        }

        protected void onVertexRemoved(Vertex vertex, IDictionary<string, object> props)
        {
            trigger.addEvent(new VertexRemovedEvent(vertex, props));
        }

        protected void onEdgeAdded(Edge edge)
        {
            trigger.addEvent(new EdgeAddedEvent(edge));
        }

        protected void onEdgeRemoved(Edge edge, IDictionary<string, object> props)
        {
            trigger.addEvent(new EdgeRemovedEvent(edge, props));
        }

        /// <note>
        /// Raises a vertexAdded event.
        /// </note>
        public Vertex addVertex(object id)
        {
            Vertex vertex = baseGraph.addVertex(id);
            if (vertex == null)
                return null;
            else
            {
                this.onVertexAdded(vertex);
                return new EventVertex(vertex, this);
            }
        }

        public Vertex getVertex(object id)
        {
            Vertex vertex = baseGraph.getVertex(id);
            if (null == vertex)
                return null;

            return new EventVertex(vertex, this);
        }

        /// <note>
        /// Raises a vertexRemoved event.
        /// </note>
        public void removeVertex(Vertex vertex)
        {
            Vertex vertexToRemove = vertex;
            if (vertex is EventVertex)
                vertexToRemove = (vertex as EventVertex).getBaseVertex();

            IDictionary<string, object> props = ElementHelper.getProperties(vertex); 
            this.baseGraph.removeVertex(vertexToRemove);
            this.onVertexRemoved(vertex, props);
        }

        public IEnumerable<Vertex> getVertices()
        {
            return new EventVertexIterable(baseGraph.getVertices(), this);
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            return new EventVertexIterable(baseGraph.getVertices(key, value), this);
        }

        /// <note>
        /// Raises an edgeAdded event.
        /// </note>
        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            Vertex outVertexToSet = outVertex;
            if (outVertex is EventVertex)
                outVertexToSet = (outVertex as EventVertex).getBaseVertex();

            Vertex inVertexToSet = inVertex;
            if (inVertex is EventVertex)
                inVertexToSet = (inVertex as EventVertex).getBaseVertex();

            Edge edge = baseGraph.addEdge(id, outVertexToSet, inVertexToSet, label);
            if (edge == null)
                return null;
            else
            {
                this.onEdgeAdded(edge);
                return new EventEdge(edge, this);
            }
        }

        public Edge getEdge(object id)
        {
            Edge edge = baseGraph.getEdge(id);
            if (null == edge)
                return null;

            return new EventEdge(edge, this);
        }

        /// <note>
        /// Raises an edgeRemoved event.
        /// </note>
        public void removeEdge(Edge edge)
        {
            Edge edgeToRemove = edge;
            if (edge is EventEdge)
                edgeToRemove = (edge as EventEdge).getBaseEdge();

            IDictionary<string, object> props = ElementHelper.getProperties(edge); 
            baseGraph.removeEdge(edgeToRemove);
            this.onEdgeRemoved(edge, props);
        }

        public IEnumerable<Edge> getEdges()
        {
            return new EventEdgeIterable(baseGraph.getEdges(), this);
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return new EventEdgeIterable(baseGraph.getEdges(key, value), this);
        }

        public GraphQuery query()
        {
            return new WrappedGraphQuery(baseGraph.query(),
                t => new EventEdgeIterable(t.edges(), this),
                t => new EventVertexIterable(t.vertices(), this));
        }

        public void shutdown()
        {
            try
            {
                baseGraph.shutdown();

                // TODO: hmmmmmm??
                trigger.fireEventQueue();
                trigger.resetEventQueue();
            }
            catch (Exception)
            {

            }
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, baseGraph.ToString());
        }

        public Graph getBaseGraph()
        {
            return baseGraph;
        }

        public Features getFeatures()
        {
            return _features;
        }
    }
}
