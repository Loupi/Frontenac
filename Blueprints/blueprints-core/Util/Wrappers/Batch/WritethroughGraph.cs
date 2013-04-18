using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    /// This is a naive wrapper to make a non-transactional graph transactional by simply writing all mutations
    /// directly through to the wrapped graph and not supporting transactional failures.
    /// <br />
    /// Hence, this is not meant as a functional implementation of a TransactionalGraph but rather as a means
    /// to using non-transactional graphs where transactional graphs are expected and transactional failure can be
    /// excluded. BatchGraph is one such case.
    /// <br />
    /// Note, the constructor will throw an exception if the given graph already supports transactions.
    /// </summary>
    public class WritethroughGraph : WrapperGraph, TransactionalGraph
    {
        readonly Graph _graph;

        public WritethroughGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentException("Graph expected");
            if (graph is TransactionalGraph)
                throw new ArgumentException("Can only wrap non-transactional graphs");
            _graph = graph;
        }

        public void rollback()
        {
            throw new InvalidOperationException("Transactions can not be rolled back");
        }

        public void commit()
        {

        }

        /// <summary>
        /// Returns the features of the underlying graph but with transactions supported.
        /// </summary>
        /// <returns>The features of the underlying graph but with transactions supported</returns>
        public Features getFeatures()
        {
            Features f = _graph.getFeatures().copyFeatures();
            f.isWrapper = true;
            f.supportsTransactions = true;
            return f;
        }

        public Vertex addVertex(object id)
        {
            return _graph.addVertex(id);
        }

        public Vertex getVertex(object id)
        {
            return _graph.getVertex(id);
        }

        public void removeVertex(Vertex vertex)
        {
            _graph.removeVertex(vertex);
        }

        public IEnumerable<Vertex> getVertices()
        {
            return _graph.getVertices();
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            return _graph.getVertices(key, value);
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return _graph.addEdge(id, outVertex, inVertex, label);
        }

        public Edge getEdge(object id)
        {
            return _graph.getEdge(id);
        }

        public void removeEdge(Edge edge)
        {
            _graph.removeEdge(edge);
        }

        public IEnumerable<Edge> getEdges()
        {
            return _graph.getEdges();
        }

        public GraphQuery query()
        {
            return _graph.query();
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return _graph.getEdges(key, value);
        }

        public void shutdown()
        {
            _graph.shutdown();
        }

        public Graph getBaseGraph()
        {
            return _graph;
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, _graph.ToString());
        }
    }
}
