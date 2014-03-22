using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public ReadOnlyEdge(ReadOnlyGraph graph, IEdge baseEdge)
            : base(graph, baseEdge)
        {
            Contract.Requires(graph != null);
            Contract.Requires(baseEdge != null);

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new ReadOnlyVertex(ReadOnlyGraph, _baseEdge.GetVertex(direction));
        }

        public string Label
        {
            get { return _baseEdge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return _baseEdge;
        }
    }
}