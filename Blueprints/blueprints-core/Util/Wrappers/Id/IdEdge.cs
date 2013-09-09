using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdge : IdElement, IEdge
    {
        readonly IEdge _baseEdge;

        public IdEdge(IEdge baseEdge, IdGraph idGraph)
            : base(baseEdge, idGraph, idGraph.GetSupportEdgeIds())
        {
            _baseEdge = baseEdge;
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return _baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new IdVertex(((IEdge)BaseElement).GetVertex(direction), IdGraph);
        }

        public string Label
        {
            get { return ((IEdge)BaseElement).Label; }
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}
