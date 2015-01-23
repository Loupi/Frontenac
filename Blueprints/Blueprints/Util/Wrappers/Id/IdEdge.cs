using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdge : IdElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public IdEdge(IEdge baseEdge, IdGraph idInnerTinkerGraĥ)
            : base(baseEdge, idInnerTinkerGraĥ, idInnerTinkerGraĥ.GetSupportEdgeIds())
        {
            Contract.Requires(baseEdge != null);
            Contract.Requires(idInnerTinkerGraĥ != null);

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new IdVertex(((IEdge) BaseElement).GetVertex(direction), IdInnerTinkerGraĥ);
        }

        public string Label
        {
            get { return ((IEdge) BaseElement).Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return _baseEdge;
        }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}