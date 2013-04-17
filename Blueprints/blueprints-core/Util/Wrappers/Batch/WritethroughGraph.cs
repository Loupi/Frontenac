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
        readonly Graph _Graph;

        public WritethroughGraph(Graph graph)
        {
            if (graph == null) throw new ArgumentException("Graph expected");
            if (graph is TransactionalGraph)
                throw new ArgumentException("Can only wrap non-transactional graphs");
            _Graph = graph;
        }

        public void Rollback()
        {
            throw new InvalidOperationException("Transactions can not be rolled back");
        }

        public void Commit()
        {

        }

        /// <summary>
        /// Returns the features of the underlying graph but with transactions supported.
        /// </summary>
        /// <returns>The features of the underlying graph but with transactions supported</returns>
        public Features GetFeatures()
        {
            Features f = _Graph.GetFeatures().CopyFeatures();
            f.IsWrapper = true;
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

        public GraphQuery Query()
        {
            return _Graph.Query();
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return _Graph.GetEdges(key, value);
        }

        public void Shutdown()
        {
            _Graph.Shutdown();
        }

        public Graph GetBaseGraph()
        {
            return _Graph;
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _Graph.ToString());
        }
    }
}
