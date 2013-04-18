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

        public static string vertexString(Vertex vertex)
        {
            return string.Concat(V, L_BRACKET, vertex.getId(), R_BRACKET);
        }

        public static string edgeString(Edge edge)
        {
            return string.Concat(E, L_BRACKET, edge.getId(), R_BRACKET, L_BRACKET, edge.getVertex(Direction.OUT).getId(), DASH, edge.getLabel(), ARROW, edge.getVertex(Direction.IN).getId(), R_BRACKET);
        }

        public static string graphString(Graph graph, string internalString)
        {
            return string.Concat(graph.GetType().Name.ToLower(), L_BRACKET, internalString, R_BRACKET);
        }

        public static string indexString(Index index)
        {
            return string.Concat("index", L_BRACKET, index.getIndexName(), COLON, index.getIndexClass().Name, R_BRACKET);
        }
    }
}
