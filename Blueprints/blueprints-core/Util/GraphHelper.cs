using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public static class GraphHelper
    {
        /// <summary>
        /// Add a vertex to the graph with specified id and provided properties.
        /// </summary>
        /// <param name="graph">the graph to create a vertex in</param>
        /// <param name="id">the id of the vertex to create</param>
        /// <param name="properties">the properties of the vertex to add (must be string,object,string,object,...)</param>
        /// <returns>the vertex created in the graph with the provided properties set</returns>
        public static Vertex AddVertex(Graph graph, object id, params object[] properties)
        {
            if ((properties.Length % 2) != 0)
                throw new ArgumentException("There must be an equal number of keys and values");

            Vertex vertex = graph.AddVertex(id);
            for (int i = 0; i < properties.Length; i = i + 2)
                vertex.SetProperty((string)properties[i], properties[i + 1]);

            return vertex;
        }

        /// <summary>
        /// Add an edge to the graph with specified id and provided properties.
        /// </summary>
        /// <param name="graph">the graph to create the edge in</param>
        /// <param name="id">the id of the edge to create</param>
        /// <param name="outVertex">the outgoing/tail vertex of the edge</param>
        /// <param name="inVertex">the incoming/head vertex of the edge</param>
        /// <param name="label">the label of the edge</param>
        /// <param name="properties">the properties of the edge to add (must be string,object,string,object,...)</param>
        /// <returns>the edge created in the graph with the provided properties set</returns>
        public static Edge AddEdge(Graph graph, object id, Vertex outVertex, Vertex inVertex, string label, params object[] properties)
        {
            if ((properties.Length % 2) != 0)
                throw new ArgumentException("There must be an equal number of keys and values");

            Edge edge = graph.AddEdge(id, outVertex, inVertex, label);
            for (int i = 0; i < properties.Length; i = i + 2)
                edge.SetProperty((string)properties[i], properties[i + 1]);

            return edge;
        }

        /// <summary>
        /// Copy the vertex/edges of one graph over to another graph.
        /// The id of the elements in the from graph are attempted to be used in the to graph.
        /// This method only works for graphs where the user can control the element ids.
        /// </summary>
        /// <param name="from">the graph to copy from</param>
        /// <param name="to">the graph to copy to</param>
        public static void CopyGraph(Graph from, Graph to)
        {
            foreach (Vertex fromVertex in from.GetVertices())
            {
                Vertex toVertex = to.AddVertex(fromVertex.GetId());
                ElementHelper.CopyProperties(fromVertex, toVertex);
            }

            foreach (Edge fromEdge in from.GetEdges())
            {
                Vertex outVertex = to.GetVertex(fromEdge.GetVertex(Direction.OUT).GetId());
                Vertex inVertex = to.GetVertex(fromEdge.GetVertex(Direction.IN).GetId());
                Edge toEdge = to.AddEdge(fromEdge.GetId(), outVertex, inVertex, fromEdge.GetLabel());
                ElementHelper.CopyProperties(fromEdge, toEdge);
            }
        }
    }
}
