using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedEdge : WrappedElement, IEdge
    {
        private readonly IEdge _edge;

        public WrappedEdge(IEdge edge)
            : base(edge)
        {
            Contract.Requires(edge != null);

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new WrappedVertex(_edge.GetVertex(direction));
        }

        public string Label => _edge.Label;

        public IEdge Edge
        {
            get
            {
                Contract.Ensures(Contract.Result<IEdge>() != null);
                return _edge;
            }
        }
    }
}