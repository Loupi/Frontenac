using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    /// <summary>
    ///     WrappedGraph serves as a template for writing a wrapper graph.
    ///     The intention is that the code in this template is copied and adjusted accordingly.
    /// </summary>
    public class WrappedGraph : IGraph, IWrapperGraph
    {
        private readonly Features _features;
        protected IGraph BaseGraph;

        public WrappedGraph(IGraph baseGraph)
        {
            Contract.Requires(baseGraph != null);

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
            var vertex = BaseGraph.GetVertex(id);
            return null == vertex ? null : new WrappedVertex(vertex);
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
            return
                new WrappedEdge(BaseGraph.AddEdge(id, ((WrappedVertex) outVertex).GetBaseVertex(),
                                                  ((WrappedVertex) inVertex).GetBaseVertex(), label));
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new WrappedEdge(edge);
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
            BaseGraph.RemoveEdge(((WrappedEdge) edge).GetBaseEdge());
        }

        public void RemoveVertex(IVertex vertex)
        {
            BaseGraph.RemoveVertex(((WrappedVertex) vertex).GetBaseVertex());
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                                    t => new WrappedEdgeIterable(t.Edges()),
                                    t => new WrappedVertexIterable(t.Vertices()));
        }

        public Features Features
        {
            get { return _features; }
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public override string ToString()
        {
            return this.GraphString(BaseGraph.ToString());
        }
    }
}