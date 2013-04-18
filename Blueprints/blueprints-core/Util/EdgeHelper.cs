using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public static class EdgeHelper
    {
        /// <summary>
        /// An edge is relabeled by creating a new edge with the same properties, but new label.
        /// Note that an edge is deleted and an edge is added.
        /// </summary>
        /// <param name="graph">the graph to add the new edge to</param>
        /// <param name="oldEdge">the existing edge to "relabel"</param>
        /// <param name="newId">the id of the new edge</param>
        /// <param name="newLabel">the label of the new edge</param>
        /// <returns>the newly created edge</returns>
        public static Edge relabelEdge(Graph graph, Edge oldEdge, object newId, string newLabel)
        {
            Vertex outVertex = oldEdge.getVertex(Direction.OUT);
            Vertex inVertex = oldEdge.getVertex(Direction.IN);
            Edge newEdge = graph.addEdge(newId, outVertex, inVertex, newLabel);
            ElementHelper.copyProperties(oldEdge, newEdge);
            graph.removeEdge(oldEdge);
            return newEdge;
        }

        /// <summary>
        /// Edges are relabeled by creating new edges with the same properties, but new label.
        /// Note that for each edge is deleted and an edge is added.
        /// </summary>
        /// <param name="graph">the graph to add the new edge to</param>
        /// <param name="oldEdges">the existing edges to "relabel"</param>
        /// <param name="newLabel">the label of the new edge</param>
        public static void relabelEdges(Graph graph, IEnumerable<Edge> oldEdges, string newLabel)
        {
            foreach (Edge oldEdge in oldEdges)
            {
                Vertex outVertex = oldEdge.getVertex(Direction.OUT);
                Vertex inVertex = oldEdge.getVertex(Direction.IN);
                Edge newEdge = graph.addEdge(null, outVertex, inVertex, newLabel);
                ElementHelper.copyProperties(oldEdge, newEdge);
                graph.removeEdge(oldEdge);
            }
        }
    }
}
