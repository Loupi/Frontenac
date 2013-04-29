using System.Collections.Generic;

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
        public static IEdge RelabelEdge(IGraph graph, IEdge oldEdge, object newId, string newLabel)
        {
            IVertex outVertex = oldEdge.GetVertex(Direction.Out);
            IVertex inVertex = oldEdge.GetVertex(Direction.In);
            IEdge newEdge = graph.AddEdge(newId, outVertex, inVertex, newLabel);
            ElementHelper.CopyProperties(oldEdge, newEdge);
            graph.RemoveEdge(oldEdge);
            return newEdge;
        }

        /// <summary>
        /// Edges are relabeled by creating new edges with the same properties, but new label.
        /// Note that for each edge is deleted and an edge is added.
        /// </summary>
        /// <param name="graph">the graph to add the new edge to</param>
        /// <param name="oldEdges">the existing edges to "relabel"</param>
        /// <param name="newLabel">the label of the new edge</param>
        public static void RelabelEdges(IGraph graph, IEnumerable<IEdge> oldEdges, string newLabel)
        {
            foreach (IEdge oldEdge in oldEdges)
            {
                IVertex outVertex = oldEdge.GetVertex(Direction.Out);
                IVertex inVertex = oldEdge.GetVertex(Direction.In);
                IEdge newEdge = graph.AddEdge(null, outVertex, inVertex, newLabel);
                ElementHelper.CopyProperties(oldEdge, newEdge);
                graph.RemoveEdge(oldEdge);
            }
        }
    }
}
