using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// An Edge links two vertices. Along with its key/value properties, an edge has both a directionality and a label.
    /// The directionality determines which vertex is the tail vertex (out vertex) and which vertex is the head vertex (in vertex).
    /// The edge label determines the type of relationship that exists between the two vertices.
    /// Diagrammatically, outVertex ---label---> inVertex.
    /// </summary>
    public interface Edge : Element
    {
        /// <summary>
        /// Return the tail/out or head/in vertex.
        /// ArgumentException is thrown if a direction of both is provided
        /// </summary>
        /// <param name="direction">whether to return the tail/out or head/in vertex</param>
        /// <returns>the tail/out or head/in vertex</returns>
        Vertex getVertex(Direction direction);

        /// <summary>
        /// Return the label associated with the edge.
        /// </summary>
        /// <returns>the edge label</returns>
        string getLabel();
    }
}
