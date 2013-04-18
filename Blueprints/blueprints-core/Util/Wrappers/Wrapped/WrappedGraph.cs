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
        protected Graph baseGraph;
        readonly Features _features;

        public WrappedGraph(Graph baseGraph)
        {
            this.baseGraph = baseGraph;
            _features = this.baseGraph.getFeatures().copyFeatures();
            _features.isWrapper = true;
        }

        public void shutdown()
        {
            baseGraph.shutdown();
        }

        public Vertex addVertex(object id)
        {
            return new WrappedVertex(baseGraph.addVertex(id));
        }

        public Vertex getVertex(object id)
        {
            Vertex vertex = baseGraph.getVertex(id);
            if (null == vertex)
                return null;

            return new WrappedVertex(vertex);
        }

        public IEnumerable<Vertex> getVertices()
        {
            return new WrappedVertexIterable(baseGraph.getVertices());
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            return new WrappedVertexIterable(baseGraph.getVertices(key, value));
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return new WrappedEdge(baseGraph.addEdge(id, ((WrappedVertex)outVertex).getBaseVertex(), ((WrappedVertex)inVertex).getBaseVertex(), label));
        }

        public Edge getEdge(object id)
        {
            Edge edge = baseGraph.getEdge(id);
            if (null == edge)
                return null;

            return new WrappedEdge(edge);
        }

        public IEnumerable<Edge> getEdges()
        {
            return new WrappedEdgeIterable(baseGraph.getEdges());
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return new WrappedEdgeIterable(baseGraph.getEdges(key, value));
        }

        public void removeEdge(Edge edge)
        {
            baseGraph.removeEdge(((WrappedEdge)edge).getBaseEdge());
        }

        public void removeVertex(Vertex vertex)
        {
            baseGraph.removeVertex(((WrappedVertex)vertex).getBaseVertex());
        }

        public Graph getBaseGraph()
        {
            return baseGraph;
        }

        public GraphQuery query()
        {
            return new WrappedGraphQuery(baseGraph.query(),
                t => new WrappedEdgeIterable(t.edges()),
                t => new WrappedVertexIterable(t.vertices()));
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, baseGraph.ToString());
        }

        public Features getFeatures()
        {
            return _features;
        }
    }
}
