﻿using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util
{
    public static class GraphHelpers
    {
        /// <summary>
        ///     Add a vertex to the graph with specified id and provided properties.
        /// </summary>
        /// <param name="graph">the graph to create a vertex in</param>
        /// <param name="id">the id of the vertex to create</param>
        /// <param name="properties">the properties of the vertex to add (must be string,object,string,object,...)</param>
        /// <returns>the vertex created in the graph with the provided properties set</returns>
        public static IVertex AddVertex(this IGraph graph, object id, params object[] properties)
        {
            Contract.Requires(graph != null);
            Contract.Requires(properties != null);
            Contract.Requires(properties.Length%2 == 0);
            Contract.Ensures(Contract.Result<IVertex>() != null);

            var vertex = graph.AddVertex(id);
            for (var i = 0; i < properties.Length; i = i + 2)
                vertex.SetProperty((string) properties[i], properties[i + 1]);

            return vertex;
        }

        /// <summary>
        ///     Add an edge to the graph with specified id and provided properties.
        /// </summary>
        /// <param name="graph">the graph to create the edge in</param>
        /// <param name="id">the id of the edge to create</param>
        /// <param name="outVertex">the outgoing/tail vertex of the edge</param>
        /// <param name="inVertex">the incoming/head vertex of the edge</param>
        /// <param name="label">the label of the edge</param>
        /// <param name="properties">the properties of the edge to add (must be string,object,string,object,...)</param>
        /// <returns>the edge created in the graph with the provided properties set</returns>
        public static IEdge AddEdge(this IGraph graph, object id, IVertex outVertex, IVertex inVertex, string label,
                                    params object[] properties)
        {
            Contract.Requires(graph != null);
            Contract.Requires(outVertex != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(properties != null);
            Contract.Requires(properties.Length%2 == 0);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            var edge = graph.AddEdge(id, outVertex, inVertex, label);
            for (var i = 0; i < properties.Length; i = i + 2)
                edge.SetProperty((string) properties[i], properties[i + 1]);

            return edge;
        }

        /// <summary>
        ///     Copy the vertex/edges of one graph over to another graph.
        ///     The id of the elements in the from graph are attempted to be used in the to graph.
        ///     This method only works for graphs where the user can control the element ids.
        /// </summary>
        /// <param name="from">the graph to copy from</param>
        /// <param name="to">the graph to copy to</param>
        public static void CopyGraph(this IGraph from, IGraph to)
        {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            foreach (var fromVertex in @from.GetVertices())
            {
                var toVertex = to.AddVertex(fromVertex.Id);
                fromVertex.CopyProperties(toVertex);
            }

            foreach (var fromEdge in from.GetEdges())
            {
                var outVertex = to.GetVertex(fromEdge.GetVertex(Direction.Out).Id);
                var inVertex = to.GetVertex(fromEdge.GetVertex(Direction.In).Id);
                var toEdge = to.AddEdge(fromEdge.Id, outVertex, inVertex, fromEdge.Label);
                fromEdge.CopyProperties(toEdge);
            }
        }
    }
}