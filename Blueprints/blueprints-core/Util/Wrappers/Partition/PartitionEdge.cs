using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public PartitionEdge(IEdge baseEdge, PartitionGraph graph)
            : base(baseEdge, graph)
        {
            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new PartitionVertex(_baseEdge.GetVertex(direction), Graph);
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