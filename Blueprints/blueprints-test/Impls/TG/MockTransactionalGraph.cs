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
        int _NumTransactionsCommitted = 0;
        int _NumTransactionsAborted = 0;

        readonly Graph _Graph;

        public MockTransactionalGraph(Graph graph)
        {
            _Graph = graph;
        }

        public void Rollback()
        {
            _NumTransactionsAborted++;
        }

        public void Commit()
        {
            _NumTransactionsCommitted++;
        }

        public int GetNumTransactionsCommitted()
        {
            return _NumTransactionsCommitted;
        }

        public int GetNumTransactionsAborted()
        {
            return _NumTransactionsAborted;
        }

        public bool AllSuccessful()
        {
            return _NumTransactionsAborted == 0;
        }

        public Features GetFeatures()
        {
            Features f = _Graph.GetFeatures().CopyFeatures();
            f.SupportsTransactions = true;
            return f;
        }

        public Vertex AddVertex(object id)
        {
            return _Graph.AddVertex(id);
        }

        public Vertex GetVertex(object id)
        {
            return _Graph.GetVertex(id);
        }

        public void RemoveVertex(Vertex vertex)
        {
            _Graph.RemoveVertex(vertex);
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return _Graph.GetVertices();
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            return _Graph.GetVertices(key, value);
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return _Graph.AddEdge(id, outVertex, inVertex, label);
        }

        public Edge GetEdge(object id)
        {
            return _Graph.GetEdge(id);
        }

        public void RemoveEdge(Edge edge)
        {
            _Graph.RemoveEdge(edge);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return _Graph.GetEdges();
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return _Graph.GetEdges(key, value);
        }

        public void Shutdown()
        {
            _Graph.Shutdown();
        }

        public GraphQuery Query()
        {
            return _Graph.Query();
        }
    }
}
