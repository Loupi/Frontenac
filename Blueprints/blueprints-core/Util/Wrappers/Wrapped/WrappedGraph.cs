using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    /// <summary>
    /// WrappedGraph serves as a template for writing a wrapper graph.
    /// The intention is that the code in this template is copied and adjusted accordingly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WrappedGraph : Graph, WrapperGraph
    {
        protected Graph _BaseGraph;
        readonly Features _Features;

        public WrappedGraph(Graph baseGraph)
        {
            _BaseGraph = baseGraph;
            _Features = _BaseGraph.GetFeatures().CopyFeatures();
            _Features.IsWrapper = true;
        }

        public void Shutdown()
        {
            _BaseGraph.Shutdown();
        }

        public Vertex AddVertex(object id)
        {
            return new WrappedVertex(_BaseGraph.AddVertex(id));
        }

        public Vertex GetVertex(object id)
        {
            Vertex vertex = _BaseGraph.GetVertex(id);
            if (null == vertex)
                return null;

            return new WrappedVertex(vertex);
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return new WrappedVertexIterable(_BaseGraph.GetVertices());
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            return new WrappedVertexIterable(_BaseGraph.GetVertices(key, value));
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return new WrappedEdge(_BaseGraph.AddEdge(id, ((WrappedVertex)outVertex).GetBaseVertex(), ((WrappedVertex)inVertex).GetBaseVertex(), label));
        }

        public Edge GetEdge(object id)
        {
            Edge edge = _BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new WrappedEdge(edge);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return new WrappedEdgeIterable(_BaseGraph.GetEdges());
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return new WrappedEdgeIterable(_BaseGraph.GetEdges(key, value));
        }

        public void RemoveEdge(Edge edge)
        {
            _BaseGraph.RemoveEdge(((WrappedEdge)edge).GetBaseEdge());
        }

        public void RemoveVertex(Vertex vertex)
        {
            _BaseGraph.RemoveVertex(((WrappedVertex)vertex).GetBaseVertex());
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public GraphQuery Query()
        {
            return new WrappedGraphQuery(_BaseGraph.Query(),
                t => new WrappedEdgeIterable(t.Edges()),
                t => new WrappedVertexIterable(t.Vertices()));
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        public Features GetFeatures()
        {
            return _Features;
        }
    }
}
