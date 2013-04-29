namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// A collection of helpful methods for creating standard toString() representations of graph-related objects.
    /// </summary>
    public static class StringFactory
    {
        public const string V = "v";
        public const string E = "e";
        public const string LBracket = "[";
        public const string RBracket = "]";
        public const string Dash = "-";
        public const string Arrow = "->";
        public const string Colon = ":";

        public const string Id = "id";
        public const string Label = "label";
        public const string EmptyString = "";

        public static string VertexString(IVertex vertex)
        {
            return string.Concat(V, LBracket, vertex.GetId(), RBracket);
        }

        public static string EdgeString(IEdge edge)
        {
            return string.Concat(E, LBracket, edge.GetId(), RBracket, LBracket, edge.GetVertex(Direction.Out).GetId(), Dash, edge.GetLabel(), Arrow, edge.GetVertex(Direction.In).GetId(), RBracket);
        }

        public static string GraphString(IGraph graph, string internalString)
        {
            return string.Concat(graph.GetType().Name.ToLower(), LBracket, internalString, RBracket);
        }

        public static string IndexString(IIndex index)
        {
            return string.Concat("index", LBracket, index.GetIndexName(), Colon, index.GetIndexClass().Name, RBracket);
        }
    }
}
