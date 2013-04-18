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
        protected Graph baseGraph;
        readonly Features _features;

        public ReadOnlyGraph(Graph baseGraph)
        {
            this.baseGraph = baseGraph;
            _features = this.baseGraph.getFeatures().copyFeatures();
            _features.isWrapper = true;
        }

        public void removeVertex(Vertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Vertex getVertex(object id)
        {
            Vertex vertex = baseGraph.getVertex(id);
            if (null == vertex)
                return null;

            return new ReadOnlyVertex(vertex);
        }

        public void removeEdge(Edge edge)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public IEnumerable<Edge> getEdges()
        {
            return new ReadOnlyEdgeIterable(baseGraph.getEdges());
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return new ReadOnlyEdgeIterable(baseGraph.getEdges(key, value));
        }

        public Edge getEdge(object id)
        {
            Edge edge = baseGraph.getEdge(id);
            if (null == edge)
                return null;

            return new ReadOnlyEdge(edge);
        }

        public IEnumerable<Vertex> getVertices()
        {
            return new ReadOnlyVertexIterable(baseGraph.getVertices());
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            return new ReadOnlyVertexIterable(baseGraph.getVertices(key, value));
        }

        public Vertex addVertex(object id)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public void shutdown()
        {
            baseGraph.shutdown();
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, baseGraph.ToString());
        }

        public Graph getBaseGraph()
        {
            return baseGraph;
        }

        public GraphQuery query()
        {
            return new WrappedGraphQuery(baseGraph.query(),
                t => new ReadOnlyEdgeIterable(t.edges()),
                t => new ReadOnlyVertexIterable(t.vertices()));
        }

        public Features getFeatures()
        {
            return _features;
        }
    }
}
