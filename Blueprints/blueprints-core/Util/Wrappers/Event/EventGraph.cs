using System;
using System.Collections.Generic;
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

        #region IDisposable members
        bool _disposed;

        ~EventGraph()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Shutdown();
            }

            _disposed = true;
        }
        #endregion

        public EventGraph(IGraph baseGraph)
        {
            BaseGraph = baseGraph;
            _features = BaseGraph.GetFeatures().CopyFeatures();
            _features.IsWrapper = true;

            Trigger = new EventTrigger(this, false);
        }

        public void RemoveAllListeners()
        {
            GraphChangedListeners.Clear();
        }

        public void AddListener(IGraphChangedListener listener)
        {
            GraphChangedListeners.Add(listener);
        }

        public IEnumerator<IGraphChangedListener> GetListenerIterator()
        {
            return GraphChangedListeners.GetEnumerator();
        }

        public EventTrigger GetTrigger()
        {
            return Trigger;
        }

        public void RemoveListener(IGraphChangedListener listener)
        {
            GraphChangedListeners.Remove(listener);
        }

        protected void OnVertexAdded(IVertex vertex)
        {
            Trigger.AddEvent(new VertexAddedEvent(vertex));
        }

        protected void OnVertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            Trigger.AddEvent(new VertexRemovedEvent(vertex, props));
        }

        protected void OnEdgeAdded(IEdge edge)
        {
            Trigger.AddEvent(new EdgeAddedEvent(edge));
        }

        protected void OnEdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            Trigger.AddEvent(new EdgeRemovedEvent(edge, props));
        }

        /// <note>
        /// Raises a vertexAdded event.
        /// </note>
        public IVertex AddVertex(object id)
        {
            IVertex vertex = BaseGraph.AddVertex(id);
            if (vertex == null)
                return null;
            OnVertexAdded(vertex);
            return new EventVertex(vertex, this);
        }

        public IVertex GetVertex(object id)
        {
            IVertex vertex = BaseGraph.GetVertex(id);
            if (null == vertex)
                return null;

            return new EventVertex(vertex, this);
        }

        /// <note>
        /// Raises a vertexRemoved event.
        /// </note>
        public void RemoveVertex(IVertex vertex)
        {
            IVertex vertexToRemove = vertex;
            if (vertex is EventVertex)
                vertexToRemove = (vertex as EventVertex).GetBaseVertex();

            IDictionary<string, object> props = ElementHelper.GetProperties(vertex); 
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
            IVertex outVertexToSet = outVertex;
            if (outVertex is EventVertex)
                outVertexToSet = (outVertex as EventVertex).GetBaseVertex();

            IVertex inVertexToSet = inVertex;
            if (inVertex is EventVertex)
                inVertexToSet = (inVertex as EventVertex).GetBaseVertex();

            IEdge edge = BaseGraph.AddEdge(id, outVertexToSet, inVertexToSet, label);
            if (edge == null)
                return null;
            OnEdgeAdded(edge);
            return new EventEdge(edge, this);
        }

        public IEdge GetEdge(object id)
        {
            IEdge edge = BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new EventEdge(edge, this);
        }

        /// <note>
        /// Raises an edgeRemoved event.
        /// </note>
        public void RemoveEdge(IEdge edge)
        {
            IEdge edgeToRemove = edge;
            if (edge is EventEdge)
                edgeToRemove = (edge as EventEdge).GetBaseEdge();

            IDictionary<string, object> props = ElementHelper.GetProperties(edge); 
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

        public IGraphQuery Query()
        {
            return new WrappedGraphQuery(BaseGraph.Query(),
                t => new EventEdgeIterable(t.Edges(), this),
                t => new EventVertexIterable(t.Vertices(), this));
        }

        void Shutdown()
        {
            try
            {
                BaseGraph.Dispose();

                // TODO: hmmmmmm??
                Trigger.FireEventQueue();
                Trigger.ResetEventQueue();
            }
            catch
            {

            }
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public Features GetFeatures()
        {
            return _features;
        }
    }
}
