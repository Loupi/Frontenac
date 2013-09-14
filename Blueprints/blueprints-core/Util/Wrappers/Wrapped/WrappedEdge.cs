using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedEdge : WrappedElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public WrappedEdge(IEdge baseEdge)
            : base(baseEdge)
        {
            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new WrappedVertex(_baseEdge.GetVertex(direction));
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