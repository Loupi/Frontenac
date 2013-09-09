using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    /// <summary>
    /// A ReadOnlyGraph wraps a Graph and overrides the underlying graph's mutating methods.
    /// In this way, a ReadOnlyGraph can only be read from, not written to.
    /// </summary>
    public class ReadOnlyGraph : IGraph, IWrapperGraph
    {
        protected IGraph BaseGraph;
        readonly Features _features;

        public ReadOnlyGraph(IGraph baseGraph)
        {
            Contract.Requires(baseGraph != null);

            BaseGraph = baseGraph;
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
        }

        public void RemoveVertex(IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertex GetVertex(object id)
        {
            var vertex = BaseGraph.GetVertex(id);
            return null == vertex ? null : new ReadOnlyVertex(vertex);
        }

        public void RemoveEdge(IEdge edge)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new ReadOnlyEdgeIterable(BaseGraph.GetEdges());
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new ReadOnlyEdgeIterable(BaseGraph.GetEdges(key, value));
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new ReadOnlyEdge(edge);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new ReadOnlyVertexIterable(BaseGraph.GetVertices());
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return new ReadOnlyVertexIterable(BaseGraph.GetVertices(key, value));
        }

        public IVertex AddVertex(object id)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                t => new ReadOnlyEdgeIterable(t.Edges()),
                t => new ReadOnlyVertexIterable(t.Vertices()));
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
