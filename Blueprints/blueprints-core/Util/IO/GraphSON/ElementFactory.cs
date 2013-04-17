using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// A factory responsible for creating graph elements.  Abstracts the way that graph elements are created. In
    /// most cases a Graph is responsible for element creation, but there are cases where more control over
    /// how vertices and edges are constructed.
    /// </summary>
    public interface ElementFactory
    {
        /// <summary>
        /// Creates a new Edge instance.
        /// </summary>
        Edge CreateEdge(object id, Vertex out_, Vertex in_, string label);

        /// <summary>
        /// reates a new Vertex instance.
        /// </summary>
        Vertex CreateVertex(object id);
    }
}
