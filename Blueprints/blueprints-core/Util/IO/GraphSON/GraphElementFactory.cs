using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// The standard factory used for most graph element creation.  It uses an actual
    /// Graph implementation to construct vertices and edges
    /// </summary>
    public class GraphElementFactory : IElementFactory
    {
        readonly IGraph _graph;

        public GraphElementFactory(IGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        public IEdge CreateEdge(object id, IVertex out_, IVertex in_, string label)
        {
            return _graph.AddEdge(id, out_, in_, label);
        }

        public IVertex CreateVertex(object id)
        {
            return _graph.AddVertex(id);
        }
    }
}
