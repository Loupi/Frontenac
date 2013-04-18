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
        public static Vertex addVertex(Graph graph, object id, params object[] properties)
        {
            if ((properties.Length % 2) != 0)
                throw new ArgumentException("There must be an equal number of keys and values");

            Vertex vertex = graph.addVertex(id);
            for (int i = 0; i < properties.Length; i = i + 2)
                vertex.setProperty((string)properties[i], properties[i + 1]);

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
        public static Edge addEdge(Graph graph, object id, Vertex outVertex, Vertex inVertex, string label, params object[] properties)
        {
            if ((properties.Length % 2) != 0)
                throw new ArgumentException("There must be an equal number of keys and values");

            Edge edge = graph.addEdge(id, outVertex, inVertex, label);
            for (int i = 0; i < properties.Length; i = i + 2)
                edge.setProperty((string)properties[i], properties[i + 1]);

            return edge;
        }

        /// <summary>
        /// Copy the vertex/edges of one graph over to another graph.
        /// The id of the elements in the from graph are attempted to be used in the to graph.
        /// This method only works for graphs where the user can control the element ids.
        /// </summary>
        /// <param name="from">the graph to copy from</param>
        /// <param name="to">the graph to copy to</param>
        public static void copyGraph(Graph from, Graph to)
        {
            foreach (Vertex fromVertex in from.getVertices())
            {
                Vertex toVertex = to.addVertex(fromVertex.getId());
                ElementHelper.copyProperties(fromVertex, toVertex);
            }

            foreach (Edge fromEdge in from.getEdges())
            {
                Vertex outVertex = to.getVertex(fromEdge.getVertex(Direction.OUT).getId());
                Vertex inVertex = to.getVertex(fromEdge.getVertex(Direction.IN).getId());
                Edge toEdge = to.addEdge(fromEdge.getId(), outVertex, inVertex, fromEdge.getLabel());
                ElementHelper.copyProperties(fromEdge, toEdge);
            }
        }
    }
}
