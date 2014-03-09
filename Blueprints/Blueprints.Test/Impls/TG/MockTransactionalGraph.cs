using System.Collections.Generic;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Mocking TinkerGraph as a transactional graph for testing purposes. This implementation does not actually
    ///     implement transactional behavior but only counts transaction starts, successes and failures so that
    ///     these can be compared to expected behavior.
    ///     This class is only meant for testing.
    /// </summary>
    public class MockTransactionalGraph : ITransactionalGraph
    {
        private readonly IGraph _graph;
        private int _numTransactionsAborted;
        private int _numTransactionsCommitted;

        public MockTransactionalGraph(IGraph graph)
        {
            _graph = graph;
        }

        public void Shutdown()
        {
            if (_graph != null)
                _graph.Shutdown();
        }

        public void Rollback()
        {
            _numTransactionsAborted++;
        }

        public void Commit()
        {
            _numTransactionsCommitted++;
        }

        public Features Features
        {
            get
            {
                Features f = _graph.Features.CopyFeatures();
                f.SupportsTransactions = true;
                return f;
            }
        }

        public IVertex AddVertex(object id)
        {
            return _graph.AddVertex(id);
        }

        public IVertex GetVertex(object id)
        {
            return _graph.GetVertex(id);
        }

        public void RemoveVertex(IVertex vertex)
        {
            _graph.RemoveVertex(vertex);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return _graph.GetVertices();
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return _graph.GetVertices(key, value);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            return _graph.AddEdge(id, outVertex, inVertex, label);
        }

        public IEdge GetEdge(object id)
        {
            return _graph.GetEdge(id);
        }

        public void RemoveEdge(IEdge edge)
        {
            _graph.RemoveEdge(edge);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return _graph.GetEdges();
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return _graph.GetEdges(key, value);
        }

        public IQuery Query()
        {
            return _graph.Query();
        }

        public int GetNumTransactionsCommitted()
        {
            return _numTransactionsCommitted;
        }

        public int GetNumTransactionsAborted()
        {
            return _numTransactionsAborted;
        }

        public bool AllSuccessful()
        {
            return _numTransactionsAborted == 0;
        }
    }
}