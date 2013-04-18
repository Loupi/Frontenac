using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// The standard factory used for most graph element creation.  It uses an actual
    /// Graph implementation to construct vertices and edges
    /// </summary>
    public class GraphElementFactory : ElementFactory
    {
        readonly Graph _graph;

        public GraphElementFactory(Graph g)
        {
            _graph = g;
        }

        public Edge createEdge(object id, Vertex out_, Vertex in_, string label)
        {
            return _graph.addEdge(id, out_, in_, label);
        }

        public Vertex createVertex(object id)
        {
            return _graph.addVertex(id);
        }
    }
}
