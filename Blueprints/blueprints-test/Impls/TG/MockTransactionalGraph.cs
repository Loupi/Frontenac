using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Mocking TinkerGraph as a transactional graph for testing purposes. This implementation does not actually
    /// implement transactional behavior but only counts transaction starts, successes and failures so that
    /// these can be compared to expected behavior.
    /// This class is only meant for testing.
    /// </summary>
    public class MockTransactionalGraph : TransactionalGraph
    {
        int _numTransactionsCommitted = 0;
        int _numTransactionsAborted = 0;

        readonly Graph _graph;

        public MockTransactionalGraph(Graph graph)
        {
            _graph = graph;
        }

        public void rollback()
        {
            _numTransactionsAborted++;
        }

        public void commit()
        {
            _numTransactionsCommitted++;
        }

        public int getNumTransactionsCommitted()
        {
            return _numTransactionsCommitted;
        }

        public int getNumTransactionsAborted()
        {
            return _numTransactionsAborted;
        }

        public bool allSuccessful()
        {
            return _numTransactionsAborted == 0;
        }

        public Features getFeatures()
        {
            Features f = _graph.getFeatures().copyFeatures();
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

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return _graph.getEdges(key, value);
        }

        public void shutdown()
        {
            _graph.shutdown();
        }

        public GraphQuery query()
        {
            return _graph.query();
        }
    }
}
