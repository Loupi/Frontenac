using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     A collection of helpful methods for creating standard toString() representations of graph-related objects.
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
            Contract.Requires(vertex != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return string.Concat(V, LBracket, vertex.Id, RBracket);
        }

        public static string EdgeString(IEdge edge)
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return string.Concat(E, LBracket, edge.Id, RBracket, LBracket, edge.GetVertex(Direction.Out).Id,
                                 Dash, edge.Label, Arrow, edge.GetVertex(Direction.In).Id, RBracket);
        }

        public static string GraphString(IGraph graph, string internalString)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(internalString));
            Contract.Ensures(Contract.Result<string>() != null);

            return string.Concat(graph.GetType().Name.ToLower(), LBracket, internalString, RBracket);
        }

        public static string IndexString(IIndex index)
        {
            Contract.Requires(index != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return string.Concat("index", LBracket, index.Name, Colon, index.Type.Name, RBracket);
        }
    }
}