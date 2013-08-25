using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    /// <summary>
    /// WrappedGraph serves as a template for writing a wrapper graph.
    /// The intention is that the code in this template is copied and adjusted accordingly.
    /// </summary>
    public class WrappedGraph : IGraph, IWrapperGraph
    {
        protected IGraph BaseGraph;
        readonly Features _features;

        public WrappedGraph(IGraph baseGraph)
        {
            BaseGraph = baseGraph;
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
        }

        public IVertex AddVertex(object id)
        {
            return new WrappedVertex(BaseGraph.AddVertex(id));
        }

        public IVertex GetVertex(object id)
        {
            IVertex vertex = BaseGraph.GetVertex(id);
            if (null == vertex)
                return null;

            return new WrappedVertex(vertex);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new WrappedVertexIterable(BaseGraph.GetVertices());
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return new WrappedVertexIterable(BaseGraph.GetVertices(key, value));
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            return new WrappedEdge(BaseGraph.AddEdge(id, ((WrappedVertex)outVertex).GetBaseVertex(), ((WrappedVertex)inVertex).GetBaseVertex(), label));
        }

        public IEdge GetEdge(object id)
        {
            IEdge edge = BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new WrappedEdge(edge);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new WrappedEdgeIterable(BaseGraph.GetEdges());
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new WrappedEdgeIterable(BaseGraph.GetEdges(key, value));
        }

        public void RemoveEdge(IEdge edge)
        {
            BaseGraph.RemoveEdge(((WrappedEdge)edge).GetBaseEdge());
        }

        public void RemoveVertex(IVertex vertex)
        {
            BaseGraph.RemoveVertex(((WrappedVertex)vertex).GetBaseVertex());
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public IGraphQuery Query()
        {
            return new WrappedGraphQuery(BaseGraph.Query(),
                t => new WrappedEdgeIterable(t.Edges()),
                t => new WrappedVertexIterable(t.Vertices()));
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }

        public Features Features
        {
            get { return _features; }
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();
        }
    }
}
