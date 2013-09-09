using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    public class EventGraph : IGraph, IWrapperGraph
    {
        protected EventTrigger Trigger;
        protected IGraph BaseGraph;
        protected readonly List<IGraphChangedListener> GraphChangedListeners = new List<IGraphChangedListener>();
        readonly Features _features;

        public EventGraph(IGraph baseGraph)
        {
            Contract.Requires(baseGraph != null);

            BaseGraph = baseGraph;
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;

            Trigger = new EventTrigger(this, false);
        }

        public void RemoveAllListeners()
        {
            GraphChangedListeners.Clear();
        }

        public void AddListener(IGraphChangedListener listener)
        {
            Contract.Requires(listener != null);

            GraphChangedListeners.Add(listener);
        }

        public IEnumerator<IGraphChangedListener> GetListenerIterator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<IGraphChangedListener>>() != null);

            return GraphChangedListeners.GetEnumerator();
        }

        public EventTrigger GetTrigger()
        {
            Contract.Ensures(Contract.Result<EventTrigger>() != null);

            return Trigger;
        }

        public void RemoveListener(IGraphChangedListener listener)
        {
            Contract.Requires(listener != null);

            GraphChangedListeners.Remove(listener);
        }

        protected void OnVertexAdded(IVertex vertex)
        {
            Contract.Requires(vertex != null);

            Trigger.AddEvent(new VertexAddedEvent(vertex));
        }

        protected void OnVertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(props != null);

            Trigger.AddEvent(new VertexRemovedEvent(vertex, props));
        }

        protected void OnEdgeAdded(IEdge edge)
        {
            Contract.Requires(edge != null);

            Trigger.AddEvent(new EdgeAddedEvent(edge));
        }

        protected void OnEdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            Contract.Requires(edge != null);
            Contract.Requires(props != null);

            Trigger.AddEvent(new EdgeRemovedEvent(edge, props));
        }

        /// <note>
        /// Raises a vertexAdded event.
        /// </note>
        public IVertex AddVertex(object id)
        {
            var vertex = BaseGraph.AddVertex(id);
            if (vertex == null)
                return null;
            OnVertexAdded(vertex);
            return new EventVertex(vertex, this);
        }

        public IVertex GetVertex(object id)
        {
            var vertex = BaseGraph.GetVertex(id);
            return null == vertex ? null : new EventVertex(vertex, this);
        }

        /// <note>
        /// Raises a vertexRemoved event.
        /// </note>
        public void RemoveVertex(IVertex vertex)
        {
            var vertexToRemove = vertex;
            if (vertex is EventVertex)
                vertexToRemove = (vertex as EventVertex).GetBaseVertex();

            var props = ElementHelper.GetProperties(vertex); 
            BaseGraph.RemoveVertex(vertexToRemove);
            OnVertexRemoved(vertex, props);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new EventVertexIterable(BaseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return new EventVertexIterable(BaseGraph.GetVertices(key, value), this);
        }

        /// <note>
        /// Raises an edgeAdded event.
        /// </note>
        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            var outVertexToSet = outVertex;
            if (outVertex is EventVertex)
                outVertexToSet = (outVertex as EventVertex).GetBaseVertex();

            var inVertexToSet = inVertex;
            if (inVertex is EventVertex)
                inVertexToSet = (inVertex as EventVertex).GetBaseVertex();

            var edge = BaseGraph.AddEdge(id, outVertexToSet, inVertexToSet, label);
            if (edge == null)
                return null;
            OnEdgeAdded(edge);
            return new EventEdge(edge, this);
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new EventEdge(edge, this);
        }

        /// <note>
        /// Raises an edgeRemoved event.
        /// </note>
        public void RemoveEdge(IEdge edge)
        {
            var edgeToRemove = edge;
            if (edge is EventEdge)
                edgeToRemove = (edge as EventEdge).GetBaseEdge();

            var props = ElementHelper.GetProperties(edge); 
            BaseGraph.RemoveEdge(edgeToRemove);
            OnEdgeRemoved(edge, props);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new EventEdgeIterable(BaseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new EventEdgeIterable(BaseGraph.GetEdges(key, value), this);
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                t => new EventEdgeIterable(t.Edges(), this),
                t => new EventVertexIterable(t.Vertices(), this));
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();

            // TODO: hmmmmmm??
            Trigger.FireEventQueue();
            Trigger.ResetEventQueue();
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public Features Features
        {
            get { return _features; }
        }
    }
}
