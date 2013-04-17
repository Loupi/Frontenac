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
        readonly Graph _Graph;

        public GraphElementFactory(Graph g)
        {
            _Graph = g;
        }

        public Edge CreateEdge(object id, Vertex out_, Vertex in_, string label)
        {
            return _Graph.AddEdge(id, out_, in_, label);
        }

        public Vertex CreateVertex(object id)
        {
            return _Graph.AddVertex(id);
        }
    }
}
