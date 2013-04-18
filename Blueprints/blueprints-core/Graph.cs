using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// A Graph is a container object for a collection of vertices and a collection edges.
    /// </summary>
    public interface Graph
    {
        /// <summary>
        /// Get the particular features of the graph implementation.
        /// Not all graph implementations are identical nor perfectly implement the Blueprints API.
        /// The Features object returned contains meta-data about numerous potential divergences between implementations.
        /// </summary>
        /// <returns>the features of this particular Graph implementation</returns>
        Features getFeatures();

        /// <summary>
        /// Create a new vertex, add it to the graph, and return the newly created vertex.
        /// The provided object identifier is a recommendation for the identifier to use.
        /// It is not required that the implementation use this identifier.
        /// </summary>
        /// <param name="id">the recommended object identifier</param>
        /// <returns>the newly created vertex</returns>
        Vertex addVertex(object id);

        /// <summary>
        /// Return the vertex referenced by the provided object identifier.
        /// If no vertex is referenced by that identifier, then return null.
        /// </summary>
        /// <param name="id">the identifier of the vertex to retrieved from the graph</param>
        /// <returns>the vertex referenced by the provided identifier or null when no such vertex exists</returns>
        Vertex getVertex(object id);

        /// <summary>
        /// Remove the provided vertex from the graph.
        /// Upon removing the vertex, all the edges by which the vertex is connected must be removed as well.
        /// </summary>
        /// <param name="vertex">the vertex to remove from the graph</param>
        void removeVertex(Vertex vertex);

        /// <summary>
        /// Return an iterable to all the vertices in the graph.
        /// If this is not possible for the implementation, then an NotSupportedException can be thrown.
        /// </summary>
        /// <returns>an iterable reference to all vertices in the graph</returns>
        IEnumerable<Vertex> getVertices();

        /// <summary>
        /// Return an iterable to all the vertices in the graph that have a particular key/value property.
        /// If this is not possible for the implementation, then a NotSupportedException can be thrown.
        /// The graph implementation should use indexing structures to make this efficient else a full vertex-filter scan is required.
        /// </summary>
        /// <param name="key">the key of vertex</param>
        /// <param name="value">the value of the vertex</param>
        /// <returns>an iterable of vertices with provided key and value</returns>
        IEnumerable<Vertex> getVertices(string key, object value);

        /// <summary>
        /// Add an edge to the graph. The added edges requires a recommended identifier, a tail vertex, an head vertex, and a label.
        /// Like adding a vertex, the provided object identifier may be ignored by the implementation.
        /// </summary>
        /// <param name="id">the recommended object identifier</param>
        /// <param name="outVertex">the vertex on the tail of the edge</param>
        /// <param name="inVertex">the vertex on the head of the edge</param>
        /// <param name="label">the label associated with the edge</param>
        /// <returns>the newly created edge</returns>
        Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label);

        /// <summary>
        /// Return the edge referenced by the provided object identifier.
        /// If no edge is referenced by that identifier, then return null.
        /// </summary>
        /// <param name="id">the identifier of the edge to retrieved from the graph</param>
        /// <returns>the edge referenced by the provided identifier or null when no such edge exists</returns>
        Edge getEdge(object id);

        /// <summary>
        /// Remove the provided edge from the graph.
        /// </summary>
        /// <param name="edge">the edge to remove from the graph</param>
        void removeEdge(Edge edge);

        /// <summary>
        /// Return an iterable to all the edges in the graph.
        /// If this is not possible for the implementation, then an NotSupportedException can be thrown.
        /// </summary>
        /// <returns>an iterable reference to all edges in the graph</returns>
        IEnumerable<Edge> getEdges();

        /// <summary>
        /// Return an iterable to all the edges in the graph that have a particular key/value property.
        /// If this is not possible for the implementation, then an NotSupportedException can be thrown.
        /// The graph implementation should use indexing structures to make this efficient else a full edge-filter scan is required.
        /// </summary>
        /// <param name="key">the key of the edge</param>
        /// <param name="value">the value of the edge</param>
        /// <returns>an iterable of edges with provided key and value</returns>
        IEnumerable<Edge> getEdges(string key, object value);

        /// <summary>
        /// Generate a query object that can be used to fine tune which edges/vertices are retrieved from the graph.
        /// </summary>
        /// <returns>a graph query object with methods for constraining which data is pulled from the underlying graph</returns>
        GraphQuery query();

        /// <summary>
        /// A shutdown function is required to properly close the graph.
        /// This is important for implementations that utilize disk-based serializations.
        /// </summary>
        void shutdown();
    }
}
