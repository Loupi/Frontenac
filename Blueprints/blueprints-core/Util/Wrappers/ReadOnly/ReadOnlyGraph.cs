using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    /// <summary>
    /// A ReadOnlyGraph wraps a Graph and overrides the underlying graph's mutating methods.
    /// In this way, a ReadOnlyGraph can only be read from, not written to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlyGraph : Graph, WrapperGraph
    {
        protected Graph _BaseGraph;
        readonly Features _Features;

        public ReadOnlyGraph(Graph baseGraph)
        {
            _BaseGraph = baseGraph;
            _Features = _BaseGraph.GetFeatures().CopyFeatures();
            _Features.IsWrapper = true;
        }

        public void RemoveVertex(Vertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Vertex GetVertex(object id)
        {
            Vertex vertex = _BaseGraph.GetVertex(id);
            if (null == vertex)
                return null;

            return new ReadOnlyVertex(vertex);
        }

        public void RemoveEdge(Edge edge)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return new ReadOnlyEdgeIterable(_BaseGraph.GetEdges());
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return new ReadOnlyEdgeIterable(_BaseGraph.GetEdges(key, value));
        }

        public Edge GetEdge(object id)
        {
            Edge edge = _BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new ReadOnlyEdge(edge);
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return new ReadOnlyVertexIterable(_BaseGraph.GetVertices());
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            return new ReadOnlyVertexIterable(_BaseGraph.GetVertices(key, value));
        }

        public Vertex AddVertex(object id)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public void Shutdown()
        {
            _BaseGraph.Shutdown();
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public GraphQuery Query()
        {
            return new WrappedGraphQuery(_BaseGraph.Query(),
                t => new ReadOnlyEdgeIterable(t.Edges()),
                t => new ReadOnlyVertexIterable(t.Vertices()));
        }

        public Features GetFeatures()
        {
            return _Features;
        }
    }
}
