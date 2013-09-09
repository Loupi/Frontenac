using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        readonly IEdge _baseEdge;

        public ReadOnlyEdge(IEdge baseEdge)
            : base(baseEdge)
        {
            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new ReadOnlyVertex(_baseEdge.GetVertex(direction));
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
