using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// A collection of helpful methods for creating standard toString() representations of graph-related objects.
    /// </summary>
    public static class StringFactory
    {
        public const string V = "v";
        public const string E = "e";
        public const string L_BRACKET = "[";
        public const string R_BRACKET = "]";
        public const string DASH = "-";
        public const string ARROW = "->";
        public const string COLON = ":";

        public const string ID = "id";
        public const string LABEL = "label";
        public const string EMPTY_STRING = "";

        public static string VertexString(Vertex vertex)
        {
            return string.Concat(V, L_BRACKET, vertex.GetId(), R_BRACKET);
        }

        public static string EdgeString(Edge edge)
        {
            return string.Concat(E, L_BRACKET, edge.GetId(), R_BRACKET, L_BRACKET, edge.GetVertex(Direction.OUT).GetId(), DASH, edge.GetLabel(), ARROW, edge.GetVertex(Direction.IN).GetId(), R_BRACKET);
        }

        public static string GraphString(Graph graph, string internalString)
        {
            return string.Concat(graph.GetType().Name.ToLower(), L_BRACKET, internalString, R_BRACKET);
        }

        public static string IndexString(Index index)
        {
            return string.Concat("index", L_BRACKET, index.GetIndexName(), COLON, index.GetIndexClass().Name, R_BRACKET);
        }
    }
}
